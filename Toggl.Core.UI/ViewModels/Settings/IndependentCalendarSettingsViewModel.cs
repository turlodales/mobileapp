using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Services;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Services;
using Toggl.Shared;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels.Settings
{
    [Preserve(AllMembers = true)]
    public sealed class IndependentCalendarSettingsViewModel : CalendarSettingsViewModel
    {
        public IndependentCalendarSettingsViewModel(
            IUserPreferences userPreferences,
            IInteractorFactory interactorFactory,
            IOnboardingStorage onboardingStorage,
            IAnalyticsService analyticsService,
            INavigationService navigationService,
            IRxActionFactory rxActionFactory,
            IPermissionsChecker permissionsChecker,
            ISchedulerProvider schedulerProvider)
                : base(userPreferences, interactorFactory, onboardingStorage, analyticsService, navigationService, rxActionFactory, permissionsChecker, schedulerProvider)
        {
        }
    }
}
