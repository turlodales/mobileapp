using System;
using System.Linq;
using System.Reactive.Disposables;
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
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels
{
    public class OnboardingViewModel : ViewModel
    {
        private readonly IPlatformInfo platformInfo;
        private readonly ITimeService timeService;
        private readonly IAnalyticsService analyticsService;
        private readonly IUserAccessManager userAccessManager;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly IInteractorFactory interactorFactory;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);

        public IObservable<bool> IsLoading { get; }

        public ViewAction ContinueWithApple { get; }
        public ViewAction ContinueWithGoogle { get; }
        public ViewAction ContinueWithEmail { get; }

        public OnboardingViewModel(
            ISchedulerProvider schedulerProvider,
            IPlatformInfo platformInfo,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            IUserAccessManager userAccessManager,
            ILastTimeUsageStorage lastTimeUsageStorage,
            IRxActionFactory rxActionFactory,
            IInteractorFactory interactorFactory,
            INavigationService navigationService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(platformInfo, nameof(platformInfo));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.platformInfo = platformInfo;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.userAccessManager = userAccessManager;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.interactorFactory = interactorFactory;

            ContinueWithApple = rxActionFactory.FromAction(continueWithApple);
            ContinueWithGoogle = rxActionFactory.FromAction(continueWithGoogle);
            ContinueWithEmail = rxActionFactory.FromAsync(continueWithEmail);

            IsLoading = isLoadingSubject
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);
        }

        public override void ViewDestroyed()
        {
            base.ViewDestroyed();
            disposeBag?.Dispose();
        }

        private void continueWithApple()
        {
            // TODO: Apple login code goes here
        }

        private void continueWithGoogle()
        {
            analyticsService.ContinueWithGoogle.Track();
            tryLoggingInWithGoogle();
        }

        private Task continueWithEmail()
        {
            if (lastTimeUsageStorage.LastLogin == null)
            {
                return Navigate<SignUpViewModel, CredentialsParameter>(CredentialsParameter.Empty);
            }
            else
            {
                return Navigate<LoginViewModel, CredentialsParameter>(CredentialsParameter.Empty);
            }
        }

        private async void tryLoggingInWithGoogle()
        {
            View?.GetGoogleToken()
                .SelectMany(userAccessManager.LoginWithGoogle)
                .Track(analyticsService.Login, AuthenticationMethod.Google)
                .Subscribe(_ => onAuthenticated(), onGoogleLoginFailure)
                .DisposedBy(disposeBag);
        }

        private async void onAuthenticated()
        {
            isLoadingSubject.OnNext(true);

            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            interactorFactory.GetCurrentUser().Execute()
                .Select(u => u.Id)
                .Subscribe(analyticsService.SetUserId)
                .DisposedBy(disposeBag);

            await UIDependencyContainer.Instance.SyncManager.ForceFullSync();

            await Navigate<MainTabBarViewModel>();
        }

        private void onGoogleLoginFailure(Exception exception)
        {
            var e = exception as GoogleLoginException;
            
            if (e == null)
            {
                isLoadingSubject.OnNext(false);
                analyticsService.UnknownSignUpFailure.Track(exception.GetType().FullName, exception.Message);
                analyticsService.TrackAnonymized(exception);
            }

            if (e != null && e.LoginWasCanceled) return;

            signUpWithGoogle();
        }

        private async void signUpWithGoogle()
        {
            var country = await confirmCountryAndTermsOfService();

            if (country == null) return;

            isLoadingSubject.OnNext(true);

            interactorFactory.GetSupportedTimezones().Execute()
                .Select(supportedTimezones =>
                    supportedTimezones.FirstOrDefault(tz => platformInfo.TimezoneIdentifier == tz)
                )
                .CombineLatest(View.GetGoogleToken(), (timezone, token) =>
                    userAccessManager.SignUpWithGoogle(token, true, (int) country.Id, timezone)
                )
                .Merge()
                .Track(analyticsService.SignUp, AuthenticationMethod.Google)
                .Subscribe(_ => onAuthenticated(), onSignUpError)
                .DisposedBy(disposeBag);
        }

        private void onSignUpError(Exception exception)
        {
            isLoadingSubject.OnNext(false);

            var e = exception as GoogleLoginException;
            if (e == null)
            {
                analyticsService.UnknownSignUpFailure.Track(exception.GetType().FullName, exception.Message);
                analyticsService.TrackAnonymized(exception);
            }
            else if (e.LoginWasCanceled) return;

            View.Alert(Resources.Oops, Resources.GenericSignUpError, Resources.Ok);
        }

        private async Task<ICountry?> confirmCountryAndTermsOfService()
            => await Navigate<TermsAndCountryViewModel, ICountry?>();
    }
}
