using Android.App;
using Android.Content.Res;

namespace Toggl.Droid.Helper
{
    public static class ActiveTheme
    {
        public static class Is
        {
            public static bool DarkTheme
            {
                get
                {
                    var uiModeFlags = Application.Context.Resources.Configuration.UiMode & UiMode.NightMask;
                    var isInDarkMode = uiModeFlags == UiMode.NightYes;
                    return isInDarkMode;
                }
            }

            public static class Not
            {
                public static bool DarkTheme => !Is.DarkTheme;
            }
        }
    }
}
