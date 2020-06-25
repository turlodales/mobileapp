using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels.Extensions;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Shared.Models;
using Toggl.Storage.Settings;
using Xamarin.Essentials;
using Email = Toggl.Shared.Email;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SsoViewModel : ViewModel
    {
        private readonly ImmutableList<string> mustLinkKeys =
            ImmutableList.Create( new [] {"confirmation_code", "email"});

        private readonly ImmutableList<string> loginWithApiTokenKeys = ImmutableList.Create( new [] {"apiToken"});
        private readonly ImmutableList<string> errorKeys = ImmutableList.Create( new [] {"ssoError"});
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly IUnauthenticatedTogglApi unauthenticatedTogglApi;
        private readonly IUserAccessManager userAccessManager;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IInteractorFactory interactorFactory;
        private readonly ITimeService timeService;
        private readonly Func<Uri, Uri, Task<WebAuthenticatorResult>> authenticateFunc;


        private readonly BehaviorSubject<bool> isLoadingSubject = new BehaviorSubject<bool>(false);
        private readonly BehaviorSubject<string> errorMessageSubject = new BehaviorSubject<string>("");
        private readonly BehaviorSubject<string> emailErrorMessageSubject = new BehaviorSubject<string>("");
        public BehaviorRelay<Email> Email { get; } = new BehaviorRelay<Email>(Shared.Email.Empty);

        public IObservable<bool> IsLoading { get; }
        public IObservable<string> ErrorMessage { get; }
        public IObservable<string> EmailErrorMessage { get; }
        public ViewAction Continue { get; }

        public SsoViewModel(
            IAnalyticsService analyticsService,
            INavigationService navigationService,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory,
            IUnauthenticatedTogglApi unauthenticatedTogglApi,
            IUserAccessManager userAccessManager,
            ILastTimeUsageStorage lastTimeUsageStorage,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory,
            ITimeService timeService,
            Func<Uri, Uri, Task<WebAuthenticatorResult>> authenticateFunc)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(unauthenticatedTogglApi, nameof(unauthenticatedTogglApi));
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(authenticateFunc, nameof(authenticateFunc));

            this.analyticsService = analyticsService;
            this.schedulerProvider = schedulerProvider;
            this.unauthenticatedTogglApi = unauthenticatedTogglApi;
            this.userAccessManager = userAccessManager;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
            this.onboardingStorage = onboardingStorage;
            this.interactorFactory = interactorFactory;
            this.timeService = timeService;
            this.authenticateFunc = authenticateFunc;

            Continue = rxActionFactory.FromAsync(getSamlConfigAndInitializeAuthFlow);

            IsLoading = isLoadingSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            ErrorMessage = errorMessageSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);

            EmailErrorMessage = emailErrorMessageSubject
                .DistinctUntilChanged()
                .AsDriver(this.schedulerProvider);
        }

        private async Task getSamlConfigAndInitializeAuthFlow()
        {
            errorMessageSubject.OnNext(string.Empty);
            if (Email.Value.IsEmpty)
            {
                emailErrorMessageSubject.OnNext(Resources.NoEmailError);
                analyticsService.LocalEmailValidationLoginCheck.Track(false);
                return;
            }
            else if (!Email.Value.IsValid)
            {
                emailErrorMessageSubject.OnNext(Resources.InvalidEmailError);
                analyticsService.LocalEmailValidationLoginCheck.Track(false);
                return;
            }
            else
            {
                emailErrorMessageSubject.OnNext(string.Empty);
                analyticsService.LocalEmailValidationLoginCheck.Track(true);
            }

            analyticsService.SsoUrlRequested.Track();

            isLoadingSubject.OnNext(true);
            try
            {
                var config = await unauthenticatedTogglApi.Auth.GetSamlConfig(Email.Value);
                analyticsService.SsoUrlOutcome.Track("Success");
                await performAuthFlow(config.SsoUrl);
            }
            catch (SamlNotConfiguredException samlNotConfiguredException)
            {
                analyticsService.SsoUrlOutcome.Track("NotConfigured");
                errorMessageSubject.OnNext(Shared.Resources.SingleSignOnError);
            }
            catch (Exception exception)
            {
                analyticsService.SsoUrlOutcome.Track("Error");
                errorMessageSubject.OnNext(Shared.Resources.SomethingWentWrongTryAgain);
            }
            finally
            {
                isLoadingSubject.OnNext(false);
            }
        }

        private async Task performAuthFlow(Uri ssoUri)
        {
            analyticsService.SsoFlowStarted.Track();

            var authResult = await authenticateFunc.Invoke(ssoUri, new Uri("togglauth://"));

            var authResultParams = authResult.Properties;
            if (authResultParams.HasAllKeys(loginWithApiTokenKeys))
            {
                analyticsService.SsoFlowOutcome.Track("LoginWithApiToken");
                await userAccessManager.LoginWithApiToken(authResultParams[loginWithApiTokenKeys[0]])
                    .Track(analyticsService.Login, AuthenticationMethod.SSO);

                await this.onLoggedIn(lastTimeUsageStorage, onboardingStorage, interactorFactory, timeService,
                    analyticsService);

                await Navigate<MainTabBarViewModel, MainTabBarParameters>(MainTabBarParameters.Default);
            }
            else if (authResultParams.HasAllKeys(mustLinkKeys))
            {
                analyticsService.SsoFlowOutcome.Track("LinkAccounts");
                var confirmationCode = authResultParams[mustLinkKeys[0]];
                var email = Shared.Email.From(authResultParams[mustLinkKeys[1]]);
                await Navigate<SsoLinkViewModel, SsoLinkParameters>(
                    SsoLinkParameters.WithEmailAndConfirmationCode(email, confirmationCode));
            }
            else
            {
                analyticsService.SsoFlowOutcome.Track("Error");
                errorMessageSubject.OnNext(Shared.Resources.SingleSignOnError);
            }
        }
    }
}