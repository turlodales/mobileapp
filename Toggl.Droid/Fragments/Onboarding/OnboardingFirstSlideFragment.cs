using Android.OS;
using Android.Views;
using Toggl.Core.UI.ViewModels;

namespace Toggl.Droid.Fragments.Onboarding
{
    public partial class OnboardingFirstSlideFragment: ReactiveTabFragment<OnboardingViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.OnboardingFirstSlideFragment, container, false);
            InitializeViews(view);

            return view;
        }
    }
}