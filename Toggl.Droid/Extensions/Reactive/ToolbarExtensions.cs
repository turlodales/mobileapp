using System;
using Android.App;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Toggl.Core.UI.Reactive;

namespace Toggl.Droid.Extensions.Reactive
{
    public static class ToolbarExtensions
    {
        public static Action<bool> NavigationEnabled(this IReactive<AppCompatActivity> reactive)
        {
            return enabled =>
            {
                reactive.Base.SupportActionBar.SetDisplayShowHomeEnabled(enabled);
                reactive.Base.SupportActionBar.SetDisplayHomeAsUpEnabled(enabled);
            };
        }
    }
}
