using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Exceptions;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class LoginViewModel : ViewModelWithInput<CredentialsParameter>
    {
        private readonly IUserAccessManager userAccessManager;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IErrorHandlingService errorHandlingService;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly ITimeService timeService;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly IInteractorFactory interactorFactory;

        private IDisposable loginDisposable;

        private readonly Subject<Unit> shakeEmailSubject = new Subject<Unit>();
        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<string> emailErrorMessageSubject = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<string> passwordErrorMessageSubject = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<string> loginErrorMessageSubject = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<bool> passwordVisibleSubject = new BehaviorSubject<bool>(false);

        private bool credentialsAreValid
             => Email.Value.IsValid && !Password.Value.IsEmpty;

        public BehaviorRelay<Email> Email { get; } = new BehaviorRelay<Email>(Shared.Email.Empty);
        public BehaviorRelay<Password> Password { get; } = new BehaviorRelay<Password>(Shared.Password.Empty);

        public IObservable<bool> PasswordVisible { get; }
        public IObservable<bool> IsLoading { get; }
        public IObservable<bool> LoginEnabled { get; }
        public IObservable<Unit> ShakeEmail { get; }
        public IObservable<string> EmailErrorMessage { get; }
        public IObservable<string> PasswordErrorMessage { get; }
        public IObservable<string> LoginErrorMessage { get; }

        public ViewAction Login { get; }
        public ViewAction SignUp { get; }
        public ViewAction ForgotPassword { get; }
        public ViewAction TogglePasswordVisibility { get; }

        public LoginViewModel(
            IUserAccessManager userAccessManager,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            ILastTimeUsageStorage lastTimeUsageStorage,
            ITimeService timeService,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory,
            IInteractorFactory interactorFactory)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(errorHandlingService, nameof(errorHandlingService));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.timeService = timeService;
            this.userAccessManager = userAccessManager;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.errorHandlingService = errorHandlingService;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.schedulerProvider = schedulerProvider;
            this.interactorFactory = interactorFactory;

            Login = rxActionFactory.FromAction(login);
            SignUp = rxActionFactory.FromAsync(signUp);
            ForgotPassword = rxActionFactory.FromAsync(forgotPassword);
            TogglePasswordVisibility = rxActionFactory.FromAction(togglePasswordVisibility);

            ShakeEmail = shakeEmailSubject.AsDriver(this.schedulerProvider);
            PasswordVisible = passwordVisibleSubject.AsObservable();

            IsLoading = isLoadingSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            EmailErrorMessage = emailErrorMessageSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            PasswordErrorMessage = passwordErrorMessageSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            LoginErrorMessage = loginErrorMessageSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            LoginEnabled = Email
                .CombineLatest(
                    Password,
                    IsLoading,
                    (email, password, isLoading) => email.IsValid && password.IsValid && !isLoading)
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);
        }

        public override Task Initialize(CredentialsParameter parameter)
        {
            Email.Accept(parameter.Email);
            Password.Accept(parameter.Password);

            return base.Initialize(parameter);
        }

        private void login()
        {
            loginErrorMessageSubject.OnNext(string.Empty);

            if (Email.Value.IsEmpty)
            {
                emailErrorMessageSubject.OnNext(Resources.NoEmailError);
                analyticsService.LocalEmailValidationLoginCheck.Track(false);
            }
            else if (!Email.Value.IsValid)
            {
                emailErrorMessageSubject.OnNext(Resources.InvalidEmailError);
                analyticsService.LocalEmailValidationLoginCheck.Track(false);
            }
            else
            {
                emailErrorMessageSubject.OnNext(string.Empty);
                analyticsService.LocalEmailValidationLoginCheck.Track(true);
            }

            if (Password.Value.IsEmpty)
            {
                passwordErrorMessageSubject.OnNext(Resources.NoPasswordError);
                analyticsService.LocalPasswordValidationLoginCheck.Track(false);
            }
            else
            {
                passwordErrorMessageSubject.OnNext(string.Empty);
                analyticsService.LocalPasswordValidationLoginCheck.Track(true);
            }

            if (!credentialsAreValid)
            {
                shakeEmailSubject.OnNext(Unit.Default);
                return;
            }

            if (isLoadingSubject.Value) return;

            isLoadingSubject.OnNext(true);

            loginDisposable =
                userAccessManager
                    .Login(Email.Value, Password.Value)
                    .Track(analyticsService.Login, AuthenticationMethod.EmailAndPassword)
                    .Subscribe(_ => onAuthenticated(), onError, onCompleted);
        }

        private Task signUp()
        {
            if (isLoadingSubject.Value)
                return Task.CompletedTask;

            var parameter = CredentialsParameter.With(Email.Value, Password.Value);
            return Navigate<SignUpViewModel, CredentialsParameter>(parameter);
        }

        private async Task forgotPassword()
        {
            if (isLoadingSubject.Value) return;

            var emailParameter = EmailParameter.With(Email.Value);
            emailParameter = await Navigate<ForgotPasswordViewModel, EmailParameter, EmailParameter>(emailParameter);
            if (emailParameter != null)
                Email.Accept(emailParameter.Email);
        }

        private void togglePasswordVisibility()
            => passwordVisibleSubject.OnNext(!passwordVisibleSubject.Value);

        private async void onAuthenticated()
        {
            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            onboardingStorage.SetIsNewUser(false);

            interactorFactory.GetCurrentUser().Execute()
                .Select(u => u.Id)
                .Subscribe(analyticsService.SetUserId);

            await UIDependencyContainer.Instance.SyncManager.ForceFullSync();

            await Navigate<MainTabBarViewModel>();
        }

        private void onError(Exception exception)
        {
            isLoadingSubject.OnNext(false);
            onCompleted();

            if (errorHandlingService.TryHandleDeprecationError(exception))
                return;

            switch (exception)
            {
                case UnauthorizedException forbidden:
                    loginErrorMessageSubject.OnNext(Resources.IncorrectEmailOrPassword);
                    analyticsService.IncorrectEmailOrPasswordLoginFailure.Track();
                    break;
                
                default:
                    analyticsService.UnknownLoginFailure.Track(exception.GetType().FullName, exception.Message);
                    analyticsService.TrackAnonymized(exception);
                    loginErrorMessageSubject.OnNext(Resources.GenericLoginError);
                    break;
            }
        }

        private void onCompleted()
        {
            loginDisposable?.Dispose();
            loginDisposable = null;
        }
    }
}
