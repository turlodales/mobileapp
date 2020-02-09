using System.Reactive.Linq;
using Foundation;
using ObjCRuntime;
using Toggl.Core.Analytics;
using Toggl.Core.UI.Helper;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class MainViewController
    {
        protected override void ConfigureKeyCommands()
        {
            base.ConfigureKeyCommands();
            AddKeyCommand(forceSyncKeyCommand);
            AddKeyCommand(startTimeEntryKeyCommand);
            AddKeyCommand(editRunningTimeEntryKeyCommand);
            AddKeyCommand(stopRunningTimeEntryKeyCommand);
            AddKeyCommand(continueLastTimeEntryKeyCommand);
            AddKeyCommand(openSettingsKeyCommand);
            AddKeyCommand(ShowMainLogKeyCommand);
            AddKeyCommand(ShowReportsKeyCommand);
            AddKeyCommand(ShowCalendarKeyCommand);
        }

        private readonly UIKeyCommand forceSyncKeyCommand = KeyCommandFactory.Create(
            title: Resources.Sync,
            image: null,
            action: new Selector(nameof(startSyncing)),
            input: "r",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand startTimeEntryKeyCommand = KeyCommandFactory.Create(
                title: Resources.StartTimeEntry,
                image: null,
                action: new Selector(nameof(showStartTimeEntry)),
                input: "n",
                modifierFlags: UIKeyModifierFlags.Command,
                propertyList: null);

        private readonly UIKeyCommand editRunningTimeEntryKeyCommand = KeyCommandFactory.Create(
            title: Resources.EditRunningTimeEntry,
            image: null,
            action: new Selector(nameof(editRunningTimeEntry)),
            input: "e",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand stopRunningTimeEntryKeyCommand = KeyCommandFactory.Create(
            title: Resources.StopRunningTimeEntry,
            image: null,
            action: new Selector(nameof(stopRunningTimeEntry)),
            input: "s",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand continueLastTimeEntryKeyCommand = KeyCommandFactory.Create(
            title: Resources.ContinueLastEntry,
            image: null,
            action: new Selector(nameof(continueLastTimeEntry)),
            input: "o",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand openSettingsKeyCommand = KeyCommandFactory.Create(
            title: Resources.Settings,
            image: null,
            action: new Selector(nameof(openSettings)),
            input: ",",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        [Export(nameof(startSyncing))]
        private void startSyncing()
        {
            ViewModel.Refresh.Execute();
        }

        [Export(nameof(showStartTimeEntry))]
        private void showStartTimeEntry()
        {
            ViewModel.StartTimeEntry.Execute(true);
        }

        [Export(nameof(editRunningTimeEntry))]
        private async void editRunningTimeEntry()
        {
            var runningTimeEntry = await ViewModel.CurrentRunningTimeEntry.FirstOrDefaultAsync();
            if (runningTimeEntry == null) return;

            var editTimeEntryInfo = new EditTimeEntryInfo(EditTimeEntryOrigin.KeyboardShortcut, runningTimeEntry.Id);
            ViewModel.SelectTimeEntry.Execute(editTimeEntryInfo);
        }

        [Export(nameof(stopRunningTimeEntry))]
        private async void stopRunningTimeEntry()
        {
            var runningTimeEntry = await ViewModel.CurrentRunningTimeEntry.FirstOrDefaultAsync();
            if (runningTimeEntry == null) return;

            ViewModel.StopTimeEntry.Execute(TimeEntryStopOrigin.Keyboard);
        }

        [Export(nameof(continueLastTimeEntry))]
        private void continueLastTimeEntry()
        {
            IosDependencyContainer.Instance.InteractorFactory.ContinueMostRecentTimeEntry().Execute();
        }

        [Export(nameof(openSettings))]
        private async void openSettings()
        {
            ViewModel.OpenSettings.Execute();
        }
    }
}
