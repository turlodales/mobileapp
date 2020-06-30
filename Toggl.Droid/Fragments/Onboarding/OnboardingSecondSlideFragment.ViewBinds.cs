using Android.Views;
using Android.Widget;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingSecondSlideFragment
    {
        private TextView onboardingMessage;
        private View pupilView;
        private View periscopeView;
        private View slideHolder;

        protected override void InitializeViews(View view)
        {
            onboardingMessage = view.FindViewById<TextView>(Resource.Id.message);
            pupilView = view.FindViewById(Resource.Id.imgPupil);
            periscopeView = view.FindViewById(Resource.Id.imgPeriscope);
            slideHolder = view.FindViewById(Resource.Id.slideHolder);

            slideHolder.FitBottomPaddingInset();
            onboardingMessage.Text = Shared.Resources.OnboardingMessagePage2;
        }
    }
}