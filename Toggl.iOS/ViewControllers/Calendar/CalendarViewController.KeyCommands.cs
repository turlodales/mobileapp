using System.Reactive.Linq;
using Foundation;
using ObjCRuntime;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class CalendarViewController
    {
        protected override void ConfigureKeyCommands()
        {
            base.ConfigureKeyCommands();
            AddKeyCommand(previousDayKeyCommand);
            AddKeyCommand(nextDayKeyCommand);
            AddKeyCommand(openSettingsKeyCommand);
            AddKeyCommand(ShowMainLogKeyCommand);
            AddKeyCommand(ShowReportsKeyCommand);
            AddKeyCommand(ShowCalendarKeyCommand);
        }

        private readonly UIKeyCommand previousDayKeyCommand = KeyCommandFactory.Create(
            title: Resources.Previous,
            image: null,
            action: new Selector(nameof(goToPreviousDay)),
            input: UIKeyCommand.LeftArrow,
            modifierFlags: default,
            propertyList: null);

        private readonly UIKeyCommand nextDayKeyCommand = KeyCommandFactory.Create(
            title: Resources.Next,
            image: null,
            action: new Selector(nameof(goToNextDay)),
            input: UIKeyCommand.RightArrow,
            modifierFlags: default,
            propertyList: null);

        private readonly UIKeyCommand openSettingsKeyCommand = KeyCommandFactory.Create(
            title: Resources.Settings,
            image: null,
            action: new Selector(nameof(openSettings)),
            input: ",",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        [Export(nameof(goToPreviousDay))]
        private async void goToPreviousDay()
        {
            var selected = ViewModel.CurrentlyShownDate.Value;
            var previous = await ViewModel.WeekViewDays.SelectMany(CommonFunctions.Identity)
                .FirstOrDefaultAsync(item => item.Date == selected.Date.AddDays(-1));

            if (previous == null || !previous.Enabled)
                return;

            ViewModel.SelectDayFromWeekView.Execute(previous);
        }

        [Export(nameof(goToNextDay))]
        private async void goToNextDay()
        {
            var selected = ViewModel.CurrentlyShownDate.Value;
            var next = await ViewModel.WeekViewDays.SelectMany(CommonFunctions.Identity)
                .FirstOrDefaultAsync(item => item.Date == selected.Date.AddDays(+1));

            if (next == null || !next.Enabled)
                return;

            ViewModel.SelectDayFromWeekView.Execute(next);
        }

        [Export(nameof(openSettings))]
        private void openSettings()
        {
            ViewModel.OpenSettings.Execute();
        }
    }
}
