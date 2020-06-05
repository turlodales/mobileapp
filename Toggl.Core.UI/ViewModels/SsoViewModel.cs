using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Shared.Models;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SsoViewModel : ViewModel
    {
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly IUnauthenticatedTogglApi unauthenticatedTogglApi;

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
            IUnauthenticatedTogglApi unauthenticatedTogglApi)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.analyticsService = analyticsService;
            this.schedulerProvider = schedulerProvider;
            this.unauthenticatedTogglApi = unauthenticatedTogglApi;

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

            isLoadingSubject.OnNext(true);
            try
            {
                var config = await unauthenticatedTogglApi.Auth.GetSamlConfig(Email.Value);
                await performAuthFlow(config.SsoUrl);
                return;
            }
            catch (SamlNotConfiguredException samlNotConfiguredException)
            {
                errorMessageSubject.OnNext(Shared.Resources.SingleSignOnError);
            }
            catch (Exception exception)
            {
                errorMessageSubject.OnNext(Shared.Resources.SomethingWentWrongTryAgain);
            }
            finally
            {
                isLoadingSubject.OnNext(false);
            }
        }

        private async Task performAuthFlow(Uri ssoUri)
        {
            var authResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(
                ssoUri,
                new Uri("togglauth://"));
        }
    }
}