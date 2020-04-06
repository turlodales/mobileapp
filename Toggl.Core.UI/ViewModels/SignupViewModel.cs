using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.UI.Extensions;
using Toggl.Core.Analytics;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Shared.Models;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SignUpViewModel : ViewModelWithInput<CredentialsParameter>
    {
        private readonly ITimeService timeService;
        private readonly IPlatformInfo platformInfo;
        private readonly IAnalyticsService analyticsService;
        private readonly IUserAccessManager userAccessManager;
        private readonly IInteractorFactory interactorFactory;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly IErrorHandlingService errorHandlingService;

        private readonly ISubject<Unit> shakeEmailFieldSubject = new Subject<Unit>();
        private readonly BehaviorSubject<bool> passwordVisibleSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<string> emailErrorSubject = new BehaviorSubject<string>(string.Empty);
        private readonly BehaviorSubject<string> passwordErrorSubject = new BehaviorSubject<string>(string.Empty);
        private readonly BehaviorSubject<string> signUpErrorSubject = new BehaviorSubject<string>(string.Empty);
        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);

        private long? countryId;
        private IDisposable signUpDisposable;

        private bool credentialsAreValid
            => Email.Value.IsValid && Password.Value.IsValid;

        public BehaviorRelay<Email> Email { get; } = new BehaviorRelay<Email>(Shared.Email.Empty);
        public BehaviorRelay<Password> Password { get; } = new BehaviorRelay<Password>(Shared.Password.Empty);

        public IObservable<bool> PasswordVisible { get; }
        public IObservable<bool> SignUpEnabled { get; }
        public IObservable<string> SignUpError { get; }
        public IObservable<string> EmailError { get; }
        public IObservable<string> PasswordError { get; }
        public IObservable<bool> IsLoading { get; }
        public IObservable<Unit> ShakeEmailField { get; }

        public ViewAction SignUp { get; }
        public ViewAction Login { get; }
        public ViewAction TogglePasswordVisibility { get; }

        public SignUpViewModel(
            ITimeService timeService,
            IPlatformInfo platformInfo,
            IRxActionFactory rxActionFactory,
            IAnalyticsService analyticsService,
            IUserAccessManager userAccessManager,
            ISchedulerProvider schedulerProvider,
            IInteractorFactory interactorFactory,
            INavigationService navigationService,
            IOnboardingStorage onboardingStorage,
            ILastTimeUsageStorage lastTimeUsageStorage,
            IErrorHandlingService errorHandlingService) : base(navigationService)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(platformInfo, nameof(platformInfo));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(errorHandlingService, nameof(errorHandlingService));

            this.timeService = timeService;
            this.platformInfo = platformInfo;
            this.analyticsService = analyticsService;
            this.userAccessManager = userAccessManager;
            this.interactorFactory = interactorFactory;
            this.onboardingStorage = onboardingStorage;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.errorHandlingService = errorHandlingService;

            SignUpEnabled = Email
                .ReemitWhen(Password.SelectUnit())
                .Select(_ => credentialsAreValid)
                .DistinctUntilChanged()
                .AsDriver(false, schedulerProvider);

            IsLoading = isLoadingSubject.DistinctUntilChanged().AsDriver(schedulerProvider);
            EmailError = emailErrorSubject.DistinctUntilChanged().AsDriver(schedulerProvider);
            SignUpError = signUpErrorSubject.DistinctUntilChanged().AsDriver(schedulerProvider);
            PasswordError = passwordErrorSubject.DistinctUntilChanged().AsDriver(schedulerProvider);
            PasswordVisible = passwordVisibleSubject.DistinctUntilChanged().AsDriver(schedulerProvider);
            ShakeEmailField = shakeEmailFieldSubject.AsDriver(schedulerProvider);

            Login = rxActionFactory.FromAsync(login);
            SignUp = rxActionFactory.FromAsync(signUp);
            TogglePasswordVisibility = rxActionFactory.FromAction(togglePasswordVisibility);
        }

        public override async Task Initialize(CredentialsParameter payload)
        {
            await base.Initialize(payload);

            Email.Accept(payload.Email);
            Password.Accept(payload.Password);
        }

        private Task login()
            => Navigate<LoginViewModel, CredentialsParameter>(CredentialsParameter.With(Email.Value, Password.Value));

        private void togglePasswordVisibility()
            => passwordVisibleSubject.OnNext(!passwordVisibleSubject.Value);

        private async Task signUp()
        {
            if (Email.Value.IsEmpty)
            {
                emailErrorSubject.OnNext(Resources.NoEmailError);
                analyticsService.LocalEmailValidationSignUpCheck.Track(false);
            }
            else if (!Email.Value.IsValid)
            {
                emailErrorSubject.OnNext(Resources.InvalidEmailError);
                analyticsService.LocalEmailValidationSignUpCheck.Track(false);
            }
            else
            {
                emailErrorSubject.OnNext(String.Empty);
                analyticsService.LocalEmailValidationSignUpCheck.Track(true);
            }

            if (Password.Value.IsEmpty)
            {
                passwordErrorSubject.OnNext(Resources.NoPasswordError);
                analyticsService.LocalPasswordValidationSignUpCheck.Track(false);
            }
            else if (!Password.Value.IsValid)
            {
                passwordErrorSubject.OnNext(Resources.InvalidPasswordError);
                analyticsService.LocalPasswordValidationSignUpCheck.Track(false);
            }
            else
            {
                passwordErrorSubject.OnNext(string.Empty);
                analyticsService.LocalPasswordValidationSignUpCheck.Track(true);
            }

            if (!credentialsAreValid)
            {
                shakeEmailFieldSubject.OnNext(Unit.Default);
                return;
            }

            countryId = await requestAcceptanceOfTermsAndConditionsAndSetCountry();

            if (countryId == null)
            {
                analyticsService.LocalCountryValidationSignUpCheck.Track(false);
                return;
            }
            else
                analyticsService.LocalCountryValidationSignUpCheck.Track(true);

            if (isLoadingSubject.Value) return;

            isLoadingSubject.OnNext(true);

            var termsOfServiceAccepted = countryId.HasValue;
            signUpDisposable = interactorFactory.GetSupportedTimezones().Execute()
                .Select(supportedTimezones => supportedTimezones.FirstOrDefault(tz => platformInfo.TimezoneIdentifier == tz))
                .SelectMany(timezone
                    => userAccessManager
                        .SignUp(
                            Email.Value,
                            Password.Value,
                            termsOfServiceAccepted,
                            (int)countryId.Value,
                            timezone)
                )
                .Track(analyticsService.SignUp, AuthenticationMethod.EmailAndPassword)
                .Do(_ =>
                {
                    var password = Password.Value;
                    if (!password.IsValid)
                        return;

                    analyticsService.Track(new SignupPasswordComplexityEvent(password));
                })
                .Subscribe(_ => onAuthenticated(), onError, onCompleted);
        }

        private async void onAuthenticated()
        {
            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            onboardingStorage.SetIsNewUser(true);
            onboardingStorage.SetUserSignedUp();

            await UIDependencyContainer.Instance.SyncManager.ForceFullSync();

            isLoadingSubject.OnNext(false);

            await Navigate<MainTabBarViewModel>();
        }

        private void onError(Exception exception)
        {
            isLoadingSubject.OnNext(false);
            onCompleted();

            if (errorHandlingService.TryHandleDeprecationError(exception))
                return;

            shakeEmailFieldSubject.OnNext(Unit.Default);

            switch (exception)
            {
                case UnauthorizedException _:
                    signUpErrorSubject.OnNext(Resources.IncorrectEmailOrPassword);
                    analyticsService.IncorrectEmailOrPasswordSignUpFailure.Track();
                    break;
                case EmailIsAlreadyUsedException _:
                    signUpErrorSubject.OnNext(Resources.EmailIsAlreadyUsedError);
                    analyticsService.EmailIsAlreadyInUsedSignUpFailure.Track();
                    break;
                default:
                    analyticsService.UnknownSignUpFailure.Track(exception.GetType().FullName, exception.Message);
                    analyticsService.TrackAnonymized(exception);
                    signUpErrorSubject.OnNext(Resources.GenericSignUpError);
                    break;
            }
        }

        private void onCompleted()
        {
            signUpDisposable?.Dispose();
            signUpDisposable = null;
        }

        private async Task<long?> requestAcceptanceOfTermsAndConditionsAndSetCountry()
        {
            return countryId ?? (await Navigate<TermsAndCountryViewModel, ICountry?>())?.Id;
        }
    }
}
