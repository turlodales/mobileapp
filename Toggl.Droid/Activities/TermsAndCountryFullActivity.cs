using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Toggl.Droid.Presentation;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class TermsAndCountryFullActivity : BaseTermsAndCountryActivity
    {
        public TermsAndCountryFullActivity() : base(Resource.Layout.TermsAndCountryActivity, Resource.Style.AppTheme, Transitions.SlideInFromRight)
        {
        }

        public TermsAndCountryFullActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void InitializeViews()
        {
            base.InitializeViews();
            
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
        }
    }
}
