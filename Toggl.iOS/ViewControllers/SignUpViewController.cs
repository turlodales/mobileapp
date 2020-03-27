using System;
using System.Reactive.Linq;
using Foundation;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class SignUpViewController : KeyboardAwareViewController<SignUpViewModel>
    {
        public SignUpViewController(SignUpViewModel vm) : base(vm, nameof(SignUpViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var loginButton = createLoginButton();
            var closeButton = new UIBarButtonItem(
                UIImage.FromBundle("icClose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            closeButton.TintColor = ColorAssets.IconTint;

            var backButton = new UIBarButtonItem("",
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            backButton.TintColor = ColorAssets.IconTint;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(loginButton);
            NavigationItem.LeftBarButtonItem = closeButton;
            NavigationItem.BackBarButtonItem = backButton;

            //E-mail
            ViewModel.Email
                .Select(email => email.ToString())
                .Subscribe(EmailTextField.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            EmailTextField.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept)
                .DisposedBy(DisposeBag);

            ViewModel.EmailError
                .Merge(ViewModel.PasswordError)
                .Merge(ViewModel.SignUpError)
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

            //Loading: disabling all interaction
            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(SignUpButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(loginButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(closeButton.Rx().Enabled())
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
                .Subscribe(PasswordTextField.Rx().Enabled())
                .DisposedBy(DisposeBag);

            //Loading: making everything look disabled
            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(LogoImageView.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(isLoading => isLoading ? Resources.Loading : Resources.SignUpTitle)
                .Subscribe(SignUpButton.Rx().Title())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(SignUpButton.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(loginButton.Rx().AnimatedAlpha())
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

            EmailTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            var keyboardHeight = e.FrameEnd.Height;
            ScrollView.ContentInset = new UIEdgeInsets(0, 0, keyboardHeight, 0);
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            ScrollView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }



        private float opacityForLoadingState(bool isLoading)
            => isLoading ? 0.6f : 1;

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

