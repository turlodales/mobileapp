using Foundation;
using ObjCRuntime;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class ReactiveViewController<TViewModel>
    {
        protected virtual bool AcceptsCancelKeyCommand { get; } = false;

        protected readonly UIKeyCommand CancelKeyCommand = KeyCommandFactory.Create(
                title: Resources.Cancel,
                image: null,
                action: new Selector(nameof(cancel)),
                input: "w",
                modifierFlags: UIKeyModifierFlags.Command,
                propertyList: null);

        protected readonly UIKeyCommand ShowMainLogKeyCommand = KeyCommandFactory.Create(
            title: Resources.Timer,
            image: null,
            action: new Selector(nameof(showMainLog)),
            input: "1",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        protected readonly UIKeyCommand ShowReportsKeyCommand = KeyCommandFactory.Create(
            title: Resources.Reports,
            image: null,
            action: new Selector(nameof(showReports)),
            input: "2",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        protected readonly UIKeyCommand ShowCalendarKeyCommand = KeyCommandFactory.Create(
            title: Resources.Calendar,
            image: null,
            action: new Selector(nameof(showCalendar)),
            input: "3",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        [Export(nameof(cancel))]
        private void cancel()
        {
            ViewModel.CloseWithDefaultResult();
        }

        [Export(nameof(showMainLog))]
        private void showMainLog()
        {
            TabBarController.SelectedIndex = 0;
        }

        [Export(nameof(showReports))]
        private void showReports()
        {
            TabBarController.SelectedIndex = 1;
        }

        [Export(nameof(showCalendar))]
        private void showCalendar()
        {
            TabBarController.SelectedIndex = 2;
        }
    }
}
