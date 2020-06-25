using System;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Services;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SsoLinkViewModel : ViewModelWithInput<SsoLinkParameters>
    {
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;
        private Email email;
        private string confirmationCode;

        public ViewAction Link { get; }

        public SsoLinkViewModel(
            IAnalyticsService analyticsService,
            INavigationService navigationService,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.analyticsService = analyticsService;
            this.schedulerProvider = schedulerProvider;

            Link = rxActionFactory.FromAsync(linkAccounts);
        }

        public override Task Initialize(SsoLinkParameters payload)
        {
            email = payload.Email;
            confirmationCode = payload.ConfirmationCode;

            return base.Initialize(payload);
        }

        private Task linkAccounts()
        {
            analyticsService.SsoLinkStarted.Track();
            return Navigate<OnboardingViewModel, OnboardingParameters>(OnboardingParameters.forAccountLinking(email, confirmationCode));
        }

        public override void Close()
        {
            analyticsService.SsoLinkCancelled.Track();
            base.Close();
        }
    }
}