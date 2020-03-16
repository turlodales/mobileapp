using System;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Activities
{
    public abstract partial class BaseTermsAndCountryActivity : ReactiveActivity<TermsAndCountryViewModel>
    {
        public BaseTermsAndCountryActivity(int layoutResId, int correctTheme, ActivityTransitionSet transitions) : base(layoutResId, correctTheme, transitions)
        {
        }

        public BaseTermsAndCountryActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            TermsOfServiceNotice.TextFormatted = new SpannableString(ViewModel.FormattedDialogText)
                .SetClickableSpan(
                    ViewModel.IndexOfPrivacyPolicy,
                    Shared.Resources.PrivacyPolicy.Length,
                    ViewModel.ViewPrivacyPolicy)
                .SetClickableSpan(
                    ViewModel.IndexOfTermsOfService,
                    Shared.Resources.TermsOfService.Length,
                    ViewModel.ViewTermsOfService);
            TermsOfServiceNotice.MovementMethod = LinkMovementMethod.Instance;
            
            ViewModel.CountryButtonTitle
                .Subscribe(CountryName.Rx().TextObserver())
                .DisposedBy(DisposeBag);
            
            AgreementButton.Rx().Tap()
                .Subscribe(ViewModel.Accept.Inputs)
                .DisposedBy(DisposeBag);
            
            DropdownMenu.Rx().Tap()
                .Subscribe(ViewModel.PickCountry.Inputs)
                .DisposedBy(DisposeBag);
            
            DropdownOutline.Rx().Tap()
                .Subscribe(ViewModel.PickCountry.Inputs)
                .DisposedBy(DisposeBag);
        }
    }
}
