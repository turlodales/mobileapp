using Android.Views;
using Android.Widget;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingThirdSlideFragment
    {
        private TextView onboardingMessage;

        protected override void InitializeViews(View view)
        {
            onboardingMessage = view.FindViewById<TextView>(Resource.Id.message);

            onboardingMessage.Text = Shared.Resources.OnboardingMessagePage3;
        }
    }
}