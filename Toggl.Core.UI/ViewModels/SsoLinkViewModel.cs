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
    public sealed class SsoLinkViewModel : ViewModelWithInput<EmailParameter>
    {
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;
        private Email email;

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

        public override Task Initialize(EmailParameter payload)
        {
            email = payload.Email;
            return base.Initialize(payload);
        }

        private Task linkAccounts()
        {
            return Navigate<OnboardingViewModel, OnboardingParameters>(OnboardingParameters.forAccountLinking(email));
        }
    }
}