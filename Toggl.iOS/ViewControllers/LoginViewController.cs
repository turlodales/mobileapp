using System;
using System.Reactive.Linq;
using CoreText;
using Foundation;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class LoginViewController : ReactiveViewController<EmailLoginViewModel>
    {
        public LoginViewController(EmailLoginViewModel vm) : base(vm, nameof(LoginViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var signUpButton = createSignUpButton();
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(signUpButton);
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
                UIImage.FromBundle("icClose"),
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());

            //E-mail
            ViewModel.Email
                .Select(email => email.ToString())
                .Subscribe(EmailTextField.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            EmailTextField.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept)
                .DisposedBy(DisposeBag);

            ViewModel.EmailErrorMessage
                .Merge(ViewModel.PasswordErrorMessage)
                .Merge(ViewModel.LoginErrorMessage)
                .Where(error => !string.IsNullOrEmpty(error))
                .SelectUnit()
                .Subscribe(EmailTextField.Rx().Shake())
                .DisposedBy(DisposeBag);

            //Password
            ViewModel.Password
                .Select(password => password.ToString().Length > 0)
                .Subscribe(ShowPasswordButton.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            PasswordTextField.Rx().Text()
                .Select(Password.From)
                .Subscribe(ViewModel.Password.Accept)
                .DisposedBy(DisposeBag);

            ViewModel.PasswordVisible
                .Skip(1)
                .Select(CommonFunctions.Invert)
                .Subscribe(PasswordTextField.Rx().SecureTextEntry())
                .DisposedBy(DisposeBag);

            ViewModel.Password
                .Select(password => password.ToString())
                .Subscribe(PasswordTextField.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            //Errors
            ViewModel.LoginEnabled
                .Subscribe(LoginButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.EmailErrorMessage
                .Subscribe(EmailErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordErrorMessage
                .Subscribe(PasswordErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.LoginErrorMessage
                .Subscribe(LoginErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            //Actions
            ShowPasswordButton.Rx()
                .BindAction(ViewModel.TogglePasswordVisibility)
                .DisposedBy(DisposeBag);

            signUpButton.Rx()
                .BindAction(ViewModel.Signup)
                .DisposedBy(DisposeBag);

            LoginButton.Rx().Tap()
                .Subscribe(_ => ViewModel.Login())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(isLoading => isLoading ? Resources.Loading : Resources.LoginTitle)
                .Subscribe(LoginButton.Rx().Title())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(LoginButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            EmailTextField.BecomeFirstResponder();
        }

        private void prepareViews()
        {
            WelcomeLabel.Text = Resources.LoginWelcomeMessage;
            EmailTextField.Placeholder = Resources.Email;
            PasswordTextField.Placeholder = Resources.Password;
            LoginButton.SetTitle(Resources.LoginTitle, UIControlState.Normal);
            prepareForgotPasswordButton();

            EmailTextField.ShouldReturn += _ =>
            {
                PasswordTextField.BecomeFirstResponder();
                return false;
            };

            PasswordTextField.ShouldReturn += _ =>
            {
                PasswordTextField.ResignFirstResponder();
                ViewModel.Login();
                return false;
            };

            ShowPasswordButton.SetupShowPasswordButton();
        }

        private void prepareForgotPasswordButton()
        {
            var forgotPasswordTitle = new NSMutableAttributedString(
                    Resources.LoginForgotPassword,
                    underlineStyle: NSUnderlineStyle.Single
                );
            forgotPasswordTitle.AddAttributes(
                new UIStringAttributes
                {
                    ForegroundColor = ColorAssets.Text,
                    Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular)
                },
                new NSRange(0, forgotPasswordTitle.Length)
            );
            ForgotPasswordButton.SetAttributedTitle(
                forgotPasswordTitle,
                UIControlState.Normal
            );
        }

        private UIButton createSignUpButton()
        {
            var buttonTitle = new NSMutableAttributedString(Resources.DoNotHaveAnAccountWithQuestionMark);
            buttonTitle.Append(new NSAttributedString(" "));
            buttonTitle.Append(new NSMutableAttributedString(Resources.SignUp, underlineStyle: NSUnderlineStyle.Single));
            buttonTitle.AddAttributes(
                new UIStringAttributes
                {
                    ForegroundColor = ColorAssets.Text,
                    Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular)
                },
                new NSRange(0, buttonTitle.Length)
            );

            var button = new UIButton();
            button.SetAttributedTitle(buttonTitle, UIControlState.Normal);
            return button;
        }
    }
}

