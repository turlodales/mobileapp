using Foundation;
using ObjCRuntime;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.ViewControllers.Common
{
    public partial class SelectorViewController<T>
    {
        private void configureKeyCommands()
        {
            AddKeyCommand(cancelKeyCommand);
        }

        private readonly UIKeyCommand cancelKeyCommand = KeyCommandFactory.Create(
            title: Resources.Cancel,
            image: null,
            action: new Selector(nameof(cancel)),
            input: "w",
            modifierFlags: UIKeyModifierFlags.Command,
            propertyList: null);


        [Export(nameof(cancel))]
        private void cancel()
        {
            this.Dismiss();
        }
    }
}
