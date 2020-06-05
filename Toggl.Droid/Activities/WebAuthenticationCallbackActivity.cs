using Android.App;
using Android.Content;
using Android.Content.PM;
using Xamarin.Essentials;

namespace Toggl.Droid.Activities
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "togglauth")]
    public class WebAuthenticationCallbackActivity: WebAuthenticatorCallbackActivity
    {

    }
}