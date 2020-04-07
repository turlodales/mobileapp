using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Toggl.Core.UI.Models;
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
        {
        }

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

            ViewModel.IsLoading
                .Invert()
                .Subscribe(notLoadingViewViews.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Do(setAnimationStatus)
                .Subscribe(loadingViewViews.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            onboardingViewPager.Rx()
                .CurrentItem()
                .DistinctUntilChanged()
                .Subscribe(onPageChanged)
                .DisposedBy(DisposeBag);
        }

        private void onPageChanged(int page)
        {
            ViewModel.OnOnboardingScroll.Execute(
                new OnboardingScrollParameters
                {
                    Action = Core.Analytics.OnboardingScrollAction.Manual,
                    Direction = Core.Analytics.OnboardingScrollDirection.None,
                    PageNumber = page
                }
            );
        }

        private void setAnimationStatus(bool isLoading)
        {
            loadingViewIndicator?.Post(() =>
            {
                if (isLoading)
                {
                    loadingAnimation?.Start();
                }
                else
                {
                    loadingAnimation?.Stop();
                }
            });
        }
    }
}
