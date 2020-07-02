using System.Threading.Tasks;
using Toggl.Core.Services;
using Toggl.Core.UI.Navigation;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Xamarin.Essentials;

namespace Toggl.Core.UI.ViewModels
{
    public sealed class YourPlanViewModel : ViewModel
    {
        public ViewAction OpenTogglWebpage { get; }

        public YourPlanViewModel(
            INavigationService navigationService,
            IRxActionFactory rxActionFactory) : base(navigationService)
        {
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            OpenTogglWebpage = rxActionFactory.FromAsync(openTogglWebpage);
        }

        private Task openTogglWebpage()
            => Browser.OpenAsync(Resources.TogglUrl, BrowserLaunchMode.SystemPreferred);
    }
}

