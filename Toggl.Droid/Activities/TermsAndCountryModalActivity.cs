using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/ModalActivityTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class TermsAndCountryModalActivity : BaseTermsAndCountryActivity
    {
        private View closeButton;
        
        public TermsAndCountryModalActivity() : base(Resource.Layout.TermsAndCountryModalActivity, Resource.Style.ModalActivityTheme, Transitions.SlideInFromBottom)
        {
        }

        public TermsAndCountryModalActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void InitializeViews()
        {
            base.InitializeViews();
            closeButton = FindViewById(Resource.Id.close);
        }

        protected override void InitializeBindings()
        {
            base.InitializeBindings();
            
            closeButton.Rx().Tap()
                .Subscribe(_ => ViewModel.CloseWithDefaultResult())
                .DisposedBy(DisposeBag);
        }
    }
}
