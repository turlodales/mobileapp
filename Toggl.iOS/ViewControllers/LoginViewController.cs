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
    public partial class LoginViewController : ReactiveViewController<LoginViewModel>
    {
        private readonly UIStringAttributes plainTextAttributes = new UIStringAttributes
        {
            ForegroundColor = ColorAssets.Text,
            Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular)
        };

        public LoginViewController(LoginViewModel vm) : base(vm, nameof(LoginViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var signUpButton = createSignUpButton();
            var closeButton = new UIBarButtonItem(
                UIImage.FromBundle("icClose"),
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(signUpButton);
            NavigationItem.LeftBarButtonItem = closeButton;

            //E-mail
            ViewModel.Email
                .Select(email => email.ToString())
                .Subscribe(EmailTextField.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            EmailTextField.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept)
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
            ViewModel.EmailErrorMessage
                .Subscribe(EmailErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordErrorMessage
                .Subscribe(PasswordErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.LoginErrorMessage
                .Subscribe(LoginErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.ShakeEmail
                .Subscribe(EmailTextField.Rx().Shake())
                .DisposedBy(DisposeBag);

            //Actions
            ShowPasswordButton.Rx()
                .BindAction(ViewModel.TogglePasswordVisibility)
                .DisposedBy(DisposeBag);

            signUpButton.Rx()
                .BindAction(ViewModel.SignUp)
                .DisposedBy(DisposeBag);

            LoginButton.Rx()
                .BindAction(ViewModel.Login)
                .DisposedBy(DisposeBag);

            ForgotPasswordButton.Rx()
                .BindAction(ViewModel.ForgotPassword)
                .DisposedBy(DisposeBag);

            //Loading: disabling all interaction
            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(LoginButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(signUpButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(closeButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(ForgotPasswordButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Subscribe(this.Rx().ModalInPresentation())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(ShowPasswordButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(EmailTextField.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(LoginButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(PasswordTextField.Rx().Enabled())
                .DisposedBy(DisposeBag);

            //Loading: making everything look disabled
            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(LogoImageView.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(isLoading => isLoading ? Resources.Loading : Resources.LoginTitle)
                .Subscribe(LoginButton.Rx().Title())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(LoginButton.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(signUpButton.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(WelcomeLabel.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(EmailTextField.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(PasswordTextField.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(ForgotPasswordButton.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);


            EmailTextField.BecomeFirstResponder();
        }

        private float opacityForLoadingState(bool isLoading)
            => isLoading ? 0.6f : 1;

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
                ViewModel.Login.Execute();
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
                plainTextAttributes,
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
                plainTextAttributes,
                new NSRange(0, buttonTitle.Length)
            );

            var button = new UIButton();
            button.SetAttributedTitle(buttonTitle, UIControlState.Normal);
            return button;
        }
    }
}

