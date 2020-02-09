using System.Reactive.Linq;
using Foundation;
using ObjCRuntime;
using Toggl.iOS.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class StartTimeEntryViewController
    {
        protected override void ConfigureKeyCommands()
        {
            base.ConfigureKeyCommands();
            AddKeyCommand(saveKeyCommand);
            AddKeyCommand(selectDateKeyCommand);
            AddKeyCommand(selectTimeKeyCommand);
            AddKeyCommand(toggleBillableKeyCommand);
        }

        private readonly UIKeyCommand saveKeyCommand = KeyCommandFactory.Create(
            title: Resources.Save,
            image: null,
            action: new Selector(nameof(save)),
            input: "\r",
            modifierFlags: default,
            propertyList: null);

        private readonly UIKeyCommand toggleBillableKeyCommand = KeyCommandFactory.Create(
            title: Resources.Billable,
            image: null,
            action: new Selector(nameof(toggleBillable)),
            input: "b",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand selectDateKeyCommand = KeyCommandFactory.Create(
            title: Resources.Startdate,
            image: null,
            action: new Selector(nameof(selectDate)),
            input: "d",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        private readonly UIKeyCommand selectTimeKeyCommand = KeyCommandFactory.Create(
            title: Resources.DateAndTime,
            image: null,
            action: new Selector(nameof(selectTime)),
            input: "t",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);

        [Export(nameof(save))]
        private void save()
        {
            ViewModel.Done.Execute();
        }

        [Export(nameof(selectDate))]
        private void selectDate()
        {
            ViewModel.SetStartDate.Execute();
        }

        [Export(nameof(selectTime))]
        private void selectTime()
        {
            ViewModel.ChangeTime.Execute();
        }

        [Export(nameof(toggleBillable))]
        private void toggleBillable()
        {
            ViewModel.ToggleBillable.Execute();
        }
    }
}
