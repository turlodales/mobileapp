using Toggl.Core.UI.Navigation;
using Toggl.Shared;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public class ConnectCalendarsPopupViewModel : ViewModelWithOutput<bool>
    {
        private readonly IOnboardingStorage onboardingStorage;

        public ConnectCalendarsPopupViewModel(
            INavigationService navigationService,
            IOnboardingStorage onboardingStorage) : base(navigationService)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));

            this.onboardingStorage = onboardingStorage;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            onboardingStorage.SetConnectCalendarsPopupWasShown();
        }
    }
}
