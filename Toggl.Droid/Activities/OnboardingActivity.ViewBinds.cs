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
        private View ssoContinueWithGoogleButton;
        private Button continueWithEmailButton;
        private Button ssoContinueWithEmailButton;
        private Button ssoButton;
        private Button ssoCancelButton;
        private TextView loginGoogleLoginLabel;
        private TextView ssoLoginGoogleLoginLabel;
        private TextView ssoLoginMessage;
        private Group notLoadingViewViews;
        private Group ssoNotLoadingViewViews;
        private Group loadingViewViews;
        private ImageView loadingViewIndicator;
        private AnimationDrawable loadingAnimation;
        private ViewPager onboardingViewPager;
        private TabLayout onboardingTabIndicator;

        protected override void InitializeViews()
        {
            notLoadingViewViews = FindViewById<Group>(Resource.Id.notLoadingViewsViewGroup);
            loadingViewViews = FindViewById<Group>(Resource.Id.loadingViewsViewGroup);
            ssoNotLoadingViewViews = FindViewById<Group>(Resource.Id.ssoNotLoadingViewsViewGroup);
            loadingViewIndicator = FindViewById<ImageView>(Resource.Id.loadingIndicator);
            continueWithGoogleButton = FindViewById(Resource.Id.continueWithGoogleButton);
            ssoContinueWithGoogleButton = FindViewById(Resource.Id.ssoContinueWithGoogleButton);
            continueWithEmailButton = FindViewById<Button>(Resource.Id.continueWithEmailButton);
            ssoContinueWithEmailButton = FindViewById<Button>(Resource.Id.ssoContinueWithEmailButton);
            ssoButton = FindViewById<Button>(Resource.Id.ssoButton);
            ssoCancelButton = FindViewById<Button>(Resource.Id.ssoCancelButton);
            loginGoogleLoginLabel = FindViewById<TextView>(Resource.Id.LoginGoogleLoginLabel);
            ssoLoginGoogleLoginLabel = FindViewById<TextView>(Resource.Id.ssoLoginGoogleLoginLabel);
            ssoLoginMessage= FindViewById<TextView>(Resource.Id.ssoLoginMessage);

            ssoContinueWithEmailButton.Text = Shared.Resources.ContinueWithEmail;
            continueWithEmailButton.Text = Shared.Resources.ContinueWithEmail;
            ssoLoginGoogleLoginLabel.Text = Shared.Resources.ContinueWithGoogle;
            loginGoogleLoginLabel.Text = Shared.Resources.ContinueWithGoogle;
            ssoButton.Text = Shared.Resources.LoginWithSso;
            ssoCancelButton.Text = Shared.Resources.Cancel;
            ssoLoginMessage.Text = Shared.Resources.LoginToEnableSso;

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
            ssoButton.FitBottomMarginInset();
            ssoCancelButton.FitBottomMarginInset();
            loadingAnimation = (AnimationDrawable) loadingViewIndicator.Drawable;
        }

        private class ScreenSlidePagerAdapter : FragmentPagerAdapter
        {
            public override int Count { get; } = 3;
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