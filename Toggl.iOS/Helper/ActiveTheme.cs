using UIKit;

namespace Toggl.iOS.Helper
{
    public static class ActiveTheme
    {
        public static class Is
        {
            public static bool DarkTheme
            {
                get
                {
                    if (!UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
                        return false;

                    var userInterfaceStyle = UIApplication.SharedApplication.KeyWindow.RootViewController.TraitCollection.UserInterfaceStyle;
                    var isInDarkMode = userInterfaceStyle == UIUserInterfaceStyle.Dark;
                    return isInDarkMode;
                }
            }
        }
    }
}
