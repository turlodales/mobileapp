using Foundation;
using ObjCRuntime;
using UIKit;

namespace Toggl.iOS.Helper
{
    public static class KeyCommandFactory
    {
        public static UIKeyCommand Create(string title, UIImage image, Selector action, string input, UIKeyModifierFlags modifierFlags, NSObject propertyList)
            => UIDevice.CurrentDevice.CheckSystemVersion(13, 0)
                ? UIKeyCommand.Create(title, image, action, input, modifierFlags, propertyList)
                : UIKeyCommand.Create(new NSString(input), modifierFlags, action);
    }
}
