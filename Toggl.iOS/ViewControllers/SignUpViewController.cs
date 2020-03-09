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
    public partial class SignUpViewController : ReactiveViewController<SignUpViewModel>
    {
        public SignUpViewController(SignUpViewModel vm) : base(vm, nameof(SignUpViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var loginButton = createLoginButton();
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(loginButton);
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
            ViewModel.EmailError
                .Subscribe(EmailErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordError
                .Subscribe(PasswordErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.SignUpError
                .Subscribe(SignUpErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.ShakeEmailField
                .Subscribe(EmailTextField.Rx().Shake())
                .DisposedBy(DisposeBag);

            //Actions
            ShowPasswordButton.Rx()
                .BindAction(ViewModel.TogglePasswordVisibility)
                .DisposedBy(DisposeBag);

            loginButton.Rx()
                .BindAction(ViewModel.Login)
                .DisposedBy(DisposeBag);

            SignUpButton.Rx()
                .BindAction(ViewModel.SignUp)
                .DisposedBy(DisposeBag);

            EmailTextField.BecomeFirstResponder();
        }

        private void prepareViews()
        {
            WelcomeLabel.Text = Resources.SignUpWelcomeMessage;
            EmailTextField.Placeholder = Resources.Email;
            PasswordTextField.Placeholder = Resources.SetPassword;
            SignUpButton.SetTitle(Resources.SignUp, UIControlState.Normal);

            EmailTextField.ShouldReturn += _ =>
            {
                PasswordTextField.BecomeFirstResponder();
                return false;
            };

            PasswordTextField.ShouldReturn += _ =>
            {
                PasswordTextField.ResignFirstResponder();
                ViewModel.SignUp.Execute();
                return false;
            };

            ShowPasswordButton.SetupShowPasswordButton();
        }

        private UIButton createLoginButton()
        {
            var buttonTitle = new NSMutableAttributedString(Resources.HaveAnAccountQuestionMark);
            buttonTitle.Append(new NSAttributedString(" "));
            buttonTitle.Append(new NSMutableAttributedString(Resources.LoginTitle, underlineStyle: NSUnderlineStyle.Single));
            buttonTitle.AddAttributes(
                new UIStringAttributes
                {
                    ForegroundColor = UIColor.Black,
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

