using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using Toggl.Droid.Extensions;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;


namespace Toggl.Droid.Activities
{
    public sealed partial class SsoLoginActivity
    {
        private TextInputLayout emailInputLayout;
        private EditText emailEditText;
        private Button continueButton;
        private TextView errorLabel;
        private TextView ssoHeader;
        private View loadingOverlay;
        private Toolbar toolbar;

        protected override void InitializeViews()
        {
            emailInputLayout = FindViewById<TextInputLayout>(Resource.Id.emailTextInputLayout);
            emailEditText = FindViewById<TextInputEditText>(Resource.Id.emailEditText);
            errorLabel = FindViewById<TextView>(Resource.Id.errorLabel);
            ssoHeader = FindViewById<TextView>(Resource.Id.ssoHeader);
            loadingOverlay = FindViewById(Resource.Id.loadingOverlay);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            continueButton = FindViewById<Button>(Resource.Id.continueButton);

            emailInputLayout.Hint = Shared.Resources.WorkEmail;
            continueButton.Text = Shared.Resources.Continue;
            ssoHeader.Text = Shared.Resources.SingleSignOn;

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = string.Empty;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            
            continueButton.FitBottomMarginInset();
        }
    }
}
