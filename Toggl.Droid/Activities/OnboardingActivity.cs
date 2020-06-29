using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Toggl.Core.Analytics;
using Toggl.Core.UI.Models;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
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
        private int oldPage = 0;
        private bool scrollWasAutomatic = false;

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

            ssoContinueWithEmailButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithEmail.Inputs)
                .DisposedBy(DisposeBag);

            continueWithGoogleButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithGoogle.Inputs)
                .DisposedBy(DisposeBag);

            ssoContinueWithGoogleButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithGoogle.Inputs)
                .DisposedBy(DisposeBag);

            ssoCancelButton.Rx().Tap()
                .Subscribe(ViewModel.SingleSignOnCancel.Inputs)
                .DisposedBy(DisposeBag);

            ssoButton.Rx().Tap()
                .Subscribe(ViewModel.SingleSignOn.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Invert()
                .Subscribe(notLoadingViewViews.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Do(setAnimationStatus)
                .Subscribe(loadingViewViews.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.IsForAccountLinking
                .Subscribe(handleAccountLinkingVisibility)
                .DisposedBy(DisposeBag);

            onboardingViewPager.Rx()
                .CurrentItem()
                .DistinctUntilChanged()
                .Subscribe(onPageChanged)
                .DisposedBy(DisposeBag);

            ViewModel.GoToNextPageObservable
                .Subscribe(_ =>
                {
                    var nextPage = (onboardingViewPager.CurrentItem + 1) % 3;
                    scrollWasAutomatic = true;
                    onboardingViewPager.SetCurrentItem(nextPage, true);
                })
                .DisposedBy(DisposeBag);
        }

        private void handleAccountLinkingVisibility(bool isForAccountLinking)
        {
            notLoadingViewViews.Visibility = (!isForAccountLinking).ToVisibility();
            ssoNotLoadingViewViews.Visibility = isForAccountLinking.ToVisibility();
        }

        private void onPageChanged(int page)
        {
            ViewModel.OnOnboardingScroll.Execute(
                new OnboardingScrollParameters
                {
                    Action = scrollWasAutomatic ? OnboardingScrollAction.Automatic : OnboardingScrollAction.Manual,
                    Direction = scrollWasAutomatic || oldPage < page ? OnboardingScrollDirection.Right : OnboardingScrollDirection.Left,
                    PageNumber = page
                }
            );

            oldPage = page;
            scrollWasAutomatic = false;
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
