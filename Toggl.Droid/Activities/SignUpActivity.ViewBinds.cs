using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Toggl.Droid.Activities
{
    public sealed partial class SignUpActivity
    {
        private TextInputLayout emailInputLayout;
        private TextInputLayout passwordInputLayout;
        private EditText emailEditText;
        private EditText passwordEditText;
        private Button signUpButton;
        private TextView loginLabel;
        private TextView welcomeMessage;
        private TextView errorLabel;
        private View loadingOverlay;
        private Toolbar toolbar;

        protected override void InitializeViews()
        {
            emailInputLayout = FindViewById<TextInputLayout>(Resource.Id.emailTextInputLayout);
            passwordInputLayout = FindViewById<TextInputLayout>(Resource.Id.passwordTextInputLayout);
            emailEditText = FindViewById<TextInputEditText>(Resource.Id.emailEditText);
            passwordEditText = FindViewById<TextInputEditText>(Resource.Id.passwordEditText);
            signUpButton = FindViewById<Button>(Resource.Id.agreementButton);
            welcomeMessage = FindViewById<TextView>(Resource.Id.welcomeMessage);
            loginLabel = FindViewById<TextView>(Resource.Id.loginButton);
            errorLabel = FindViewById<TextView>(Resource.Id.errorLabel);
            loadingOverlay = FindViewById(Resource.Id.loadingOverlay);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            welcomeMessage.Text = Shared.Resources.SignUpWelcomeMessage;
            emailInputLayout.Hint = Shared.Resources.Email;
            passwordInputLayout.Hint = Shared.Resources.Password;
            signUpButton.Text = Shared.Resources.SignUp;

            var loginLabelText = new SpannableStringBuilder();
            loginLabelText.Append(Shared.Resources.HaveAnAccountQuestionMark, new TypefaceSpan(Typeface.SansSerif), SpanTypes.ExclusiveExclusive);
            loginLabelText.Append(" ");
            loginLabelText.Append(Shared.Resources.LoginTitle, new TypefaceSpan("sans-serif-medium"), SpanTypes.ExclusiveExclusive);
            loginLabelText.SetSpan(new UnderlineSpan(), Shared.Resources.HaveAnAccountQuestionMark.Length + 1, loginLabelText.Length(), SpanTypes.ExclusiveExclusive);

            loginLabel.TextFormatted = loginLabelText;
            
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = string.Empty;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            
            signUpButton.FitBottomMarginInset();
        }
    }
}
