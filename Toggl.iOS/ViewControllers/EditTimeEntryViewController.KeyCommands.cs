using Foundation;
using ObjCRuntime;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class EditTimeEntryViewController
    {
        protected override void ConfigureKeyCommands()
        {
            base.ConfigureKeyCommands();
            AddKeyCommand(saveKeyCommand);
        }

        private readonly UIKeyCommand saveKeyCommand = KeyCommandFactory.Create(
            title: Resources.Save,
            image: null,
            action: new Selector(nameof(save)),
            input: "\r",
            modifierFlags: default,
            propertyList: null);

        [Export(nameof(save))]
        private void save()
        {
            ViewModel.Save.Execute();
        }
    }
}
