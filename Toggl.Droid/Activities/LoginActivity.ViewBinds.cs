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
    public sealed partial class LoginActivity
    {
        private TextInputLayout emailInputLayout;
        private TextInputLayout passwordInputLayout;
        private EditText emailEditText;
        private EditText passwordEditText;
        private Button loginButton;
        private TextView signUpLabel;
        private TextView forgotPasswordLabel;
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
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            welcomeMessage = FindViewById<TextView>(Resource.Id.welcomeMessage);
            signUpLabel = FindViewById<TextView>(Resource.Id.signupButton);
            forgotPasswordLabel = FindViewById<TextView>(Resource.Id.forgotPasswordButton);
            errorLabel = FindViewById<TextView>(Resource.Id.errorLabel);
            loadingOverlay = FindViewById(Resource.Id.loadingOverlay);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            welcomeMessage.Text = Shared.Resources.LoginWelcomeMessage;
            emailInputLayout.Hint = Shared.Resources.Email;
            passwordInputLayout.Hint = Shared.Resources.Password;

            var signUpLabelText = new SpannableStringBuilder();
            signUpLabelText.Append(Shared.Resources.DoNotHaveAnAccountWithQuestionMark, new TypefaceSpan("sans-serif"), SpanTypes.ExclusiveExclusive);
            signUpLabelText.Append(" ");
            signUpLabelText.Append(Shared.Resources.SignUp, new TypefaceSpan("sans-serif-medium"), SpanTypes.ExclusiveExclusive);
            signUpLabelText.SetSpan(new UnderlineSpan(), Shared.Resources.DoNotHaveAnAccountWithQuestionMark.Length + 1, signUpLabelText.Length(), SpanTypes.ExclusiveExclusive);
            signUpLabel.TextFormatted = signUpLabelText;
            
            var forgotPasswordText = new SpannableStringBuilder(Shared.Resources.LoginForgotPassword);
            forgotPasswordText.SetSpan(new UnderlineSpan(), 0, forgotPasswordText.Length(), SpanTypes.ExclusiveExclusive);
            forgotPasswordLabel.TextFormatted = forgotPasswordText;
            
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = string.Empty;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            
            loginButton.FitBottomMarginInset();
        }
    }
}
