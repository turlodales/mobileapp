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
    public sealed class SsoLinkViewModel : ViewModel
    {
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;

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

        private Task linkAccounts()
        {
            return Navigate<OnboardingViewModel, OnboardingParameters>(OnboardingParameters.ForAccountLinking);
        }
    }
}