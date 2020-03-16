using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public partial class OnboardingActivity : ReactiveActivity<OnboardingViewModel>
    {
        public OnboardingActivity() : base(
            Resource.Layout.OnboardingActivity, Resource.Style.AppTheme_Onboarding, Transitions.Fade
            )
        { }

        public OnboardingActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            continueWithEmailButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithEmail.Inputs)
                .DisposedBy(DisposeBag);
            
            continueWithGoogleButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithGoogle.Inputs)
                .DisposedBy(DisposeBag);
        }
    }
}
