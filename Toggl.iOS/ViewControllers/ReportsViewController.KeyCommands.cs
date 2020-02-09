using Foundation;
using ObjCRuntime;
using Toggl.Core.Analytics;
using Toggl.Core.Models;
using Toggl.iOS.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;
using static Toggl.Core.Models.DateRangePeriod;
using static Toggl.Core.UI.ViewModels.DateRangePicker.DateRangePickerViewModel;

namespace Toggl.iOS.ViewControllers
{
    public partial class ReportsViewController
    {
        protected override void ConfigureKeyCommands()
        {
            base.ConfigureKeyCommands();
            AddKeyCommand(shortcutCommand("1", Today));
            AddKeyCommand(shortcutCommand("2", Yesterday));
            AddKeyCommand(shortcutCommand("3", ThisWeek));
            AddKeyCommand(shortcutCommand("4", LastWeek));
            AddKeyCommand(shortcutCommand("5", ThisMonth));
            AddKeyCommand(shortcutCommand("6", LastMonth));
            AddKeyCommand(shortcutCommand("7", ThisYear));
            AddKeyCommand(shortcutCommand("8", LastYear));
            AddKeyCommand(selectDateRangeCommand);
            AddKeyCommand(selectWorkspaceCommand);
            AddKeyCommand(ShowMainLogKeyCommand);
            AddKeyCommand(ShowReportsKeyCommand);
            AddKeyCommand(ShowCalendarKeyCommand);
        }

        private readonly UIKeyCommand selectWorkspaceCommand = KeyCommandFactory.Create(
            title: Resources.Workspace,
            image: null,
            action: new Selector(nameof(selectWorkspace)),
            input: "w",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private UIKeyCommand shortcutCommand(string input, DateRangePeriod period)
            => KeyCommandFactory.Create(
            title: period.ToHumanReadableString(),
            image: null,
            action: new Selector("selectShortcut:"),
            input: input,
            modifierFlags: UIKeyModifierFlags.Command | UIKeyModifierFlags.Alternate,
            propertyList: new NSNumber((int)period));

        private readonly UIKeyCommand selectDateRangeCommand = KeyCommandFactory.Create(
            title: Resources.DateRange,
            image: null,
            action: new Selector(nameof(selectDateRange)),
            input: "d",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        [Export(nameof(selectWorkspace))]
        private void selectWorkspace()
        {
            ViewModel.SelectWorkspace.Execute();
        }

        [Export(nameof(selectDateRange))]
        private void selectDateRange()
        {
            ViewModel.SelectTimeRange.Execute();
        }

        [Export("selectShortcut:")]
        private void selectShortcut(UIKeyCommand command)
        {
            var period = (DateRangePeriod)((NSNumber)command.PropertyList).Int32Value;
            var shortcut = IosDependencyContainer.Instance
                .DateRangeShortcutsService
                .GetShortcutFrom(period);

            var result = new DateRangeSelectionResult(
                shortcut.DateRange,
                shortcut.Period.ToDateRangeSelectionSource());
            ViewModel.SetTimeRange.Execute(result);
        }
    }
}
