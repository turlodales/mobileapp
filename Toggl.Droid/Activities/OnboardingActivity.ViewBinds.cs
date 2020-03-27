using System;
using Android.Runtime;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Toggl.Droid.Extensions;
using Toggl.Droid.Fragments.Onboarding;

namespace Toggl.Droid.Activities
{
    public partial class OnboardingActivity
    {
        private View continueWithGoogleButton;
        private Button continueWithEmailButton;
        private TextView loginGoogleLoginLabel;
        private Group notLoadingViewViews;
        private ImageView loadingViewIndicator;
        private AnimationDrawable loadingAnimation;
        private ViewPager onboardingViewPager;
        private TabLayout onboardingTabIndicator;

        protected override void InitializeViews()
        {
            notLoadingViewViews = FindViewById<Group>(Resource.Id.notLoadingViewsViewGroup);
            loadingViewIndicator = FindViewById<ImageView>(Resource.Id.loadingIndicator);
            continueWithGoogleButton = FindViewById(Resource.Id.continueWithGoogleButton);
            continueWithEmailButton = FindViewById<Button>(Resource.Id.continueWithEmailButton);
            loginGoogleLoginLabel = FindViewById<TextView>(Resource.Id.LoginGoogleLoginLabel);

            continueWithEmailButton.Text = Shared.Resources.ContinueWithEmail;
            loginGoogleLoginLabel.Text = Shared.Resources.ContinueWithGoogle;

            onboardingViewPager = FindViewById<ViewPager>(Resource.Id.onboardingViewPager);
            onboardingTabIndicator = FindViewById<TabLayout>(Resource.Id.onboardingTabIndicator);

            var onboardingHolder = FindViewById(Resource.Id.onboardingHolder);
            onboardingHolder.DoOnApplyWindowInsets(( view,  insets,  paddingMargin) =>
                {
                    insets.ReplaceSystemWindowInsets(insets.SystemWindowInsetLeft, 0, insets.SystemWindowInsetRight,
                        insets.SystemWindowInsetBottom);
                }
            );

            onboardingViewPager.Adapter = new ScreenSlidePagerAdapter(SupportFragmentManager);
            onboardingTabIndicator.SetupWithViewPager(onboardingViewPager);
            continueWithEmailButton.FitBottomMarginInset();
            loadingAnimation = (AnimationDrawable) loadingViewIndicator.Drawable;
        }

        private class ScreenSlidePagerAdapter : FragmentPagerAdapter
        {
            public override int Count { get; } = 1;
            public ScreenSlidePagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public ScreenSlidePagerAdapter(FragmentManager fm) : base(fm)
            {
            }

            public override Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new OnboardingFirstSlideFragment();
                    case 1:
                        return new OnboardingSecondSlideFragment();
                    case 2:
                        return new OnboardingThirdSlideFragment();
                }
                return null;
            }
        }
    }
}