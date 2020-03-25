using Android.Views;
using Android.Widget;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingFirstSlideFragment
    {
        private TextView onboardingMessage;
        private View slideHolder;

        protected override void InitializeViews(View view)
        {
            onboardingMessage = view.FindViewById<TextView>(Resource.Id.message);
            slideHolder = view.FindViewById(Resource.Id.slideHolder);

            slideHolder.FitBottomPaddingInset();
            onboardingMessage.Text = Shared.Resources.OnboardingMessagePage1;
        }
    }
}