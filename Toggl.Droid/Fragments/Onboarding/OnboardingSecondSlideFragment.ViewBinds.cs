using Android.Views;
using Android.Widget;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingSecondSlideFragment
    {
        private TextView onboardingMessage;
        private View pupilView;
        private View periscopeView;

        protected override void InitializeViews(View view)
        {
            onboardingMessage = view.FindViewById<TextView>(Resource.Id.message);
            pupilView = view.FindViewById(Resource.Id.imgPupil);
            periscopeView = view.FindViewById(Resource.Id.imgPeriscope);

            onboardingMessage.Text = Shared.Resources.OnboardingMessagePage2;
        }
    }
}