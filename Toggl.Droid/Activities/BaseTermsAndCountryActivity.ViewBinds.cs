using Android.Views;
using Android.Widget;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.Activities
{
    public abstract partial class BaseTermsAndCountryActivity
    {
        protected TextView WelcomeMessage;
        protected TextView CountryOfResidenceLegend;
        protected TextView CountryName;
        protected TextView TermsOfServiceNotice;
        protected Button AgreementButton;
        protected View DropdownMenu;
        protected View DropdownOutline;

        protected override void InitializeViews()
        {
            WelcomeMessage = FindViewById<TextView>(Resource.Id.welcome_message);
            CountryOfResidenceLegend = FindViewById<TextView>(Resource.Id.country_of_residence_label);
            CountryName = FindViewById<TextView>(Resource.Id.country_name);
            TermsOfServiceNotice = FindViewById<TextView>(Resource.Id.terms_message);
            AgreementButton = FindViewById<Button>(Resource.Id.agreement_button);
            DropdownMenu = FindViewById(Resource.Id.dropdown_menu);
            DropdownOutline = FindViewById(Resource.Id.dropdown_outline);

            WelcomeMessage.Text = Shared.Resources.ReviewTheTerms;
            CountryOfResidenceLegend.Text = Shared.Resources.CountryOfResidence;
            AgreementButton.Text = Shared.Resources.IAgree;
            AgreementButton.FitBottomMarginInset();
        }
    }
}
