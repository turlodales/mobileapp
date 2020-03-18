using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using Toggl.Droid.Extensions;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Toggl.Droid.Activities
{
    public partial class ForgotPasswordActivity
    {
        private TextView informationMessage;
        private TextInputLayout loginEmail;
        private EditText loginEmailEditText;
        private Button resetPasswordButton;
        private View loadingOverlay;
        private Toolbar toolbar;

        protected override void InitializeViews()
        {
            informationMessage = FindViewById<TextView>(Resource.Id.informationMessage);
            loginEmail = FindViewById<TextInputLayout>(Resource.Id.emailTextInputLayout);
            loginEmailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            resetPasswordButton = FindViewById<Button>(Resource.Id.resetPasswordButton);
            loadingOverlay = FindViewById(Resource.Id.loadingOverlay);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            loginEmailEditText.SetFocus();
            loginEmailEditText.SetSelection(loginEmailEditText.Text?.Length ?? 0);

            informationMessage.Text = Shared.Resources.PasswordResetMessage;
            loginEmail.HelperText = Shared.Resources.PasswordResetExplanation;
            loginEmail.Hint = Shared.Resources.Email;
            resetPasswordButton.Text = Shared.Resources.GetPasswordResetLink;
            
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = string.Empty;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
        }
    }
}
