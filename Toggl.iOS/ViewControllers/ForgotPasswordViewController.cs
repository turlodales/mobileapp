using System;
using System.Reactive.Linq;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public sealed partial class ForgotPasswordViewController : KeyboardAwareViewController<ForgotPasswordViewModel>
    {
        private bool viewInitialized;

        public ForgotPasswordViewController(ForgotPasswordViewModel viewModel)
            : base(viewModel, nameof(ForgotPasswordViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ResetPasswordButton.SetTitle(Resources.SendEmail, UIControlState.Normal);
            EmailTextField.Placeholder = Resources.EmailAddress;
            SuccessMessageLabel.Text = Resources.PasswordResetSuccess;

            prepareViews();

            //Text
            ViewModel.ErrorMessage
                .Subscribe(errorMessage =>
                {
                    ErrorLabel.Text = errorMessage;
                    ErrorLabel.Hidden = string.IsNullOrEmpty(errorMessage);
                })
                .DisposedBy(DisposeBag);

            ViewModel.Email
                .Take(1)
                .Select(email => email.ToString())
                .Subscribe(EmailTextField.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            EmailTextField.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.OnNext)
                .DisposedBy(DisposeBag);

            ViewModel.Reset.Executing
                .Subscribe(loading =>
                {
                    UIView.Transition(
                        ResetPasswordButton,
                        Animation.Timings.EnterTiming,
                        UIViewAnimationOptions.TransitionCrossDissolve,
                        () => ResetPasswordButton.SetTitle(loading ? "" : Resources.SendEmail, UIControlState.Normal),
                        null
                    );
                })
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetWithInvalidEmail
                .Subscribe(_ =>
                {
                    ErrorLabel.Text = Resources.InvalidEmailError;
                    ErrorLabel.Hidden = false;
                })
                .DisposedBy(DisposeBag);

            //Visibility
            ViewModel.PasswordResetSuccessful
                .Subscribe(DoneCard.Rx().IsVisibleWithFade())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetSuccessful
                .Invert()
                .Subscribe(ResetPasswordButton.Rx().IsVisibleWithFade())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetSuccessful
                .Where(s => s == false)
                .Subscribe(_ => EmailTextField.BecomeFirstResponder())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetSuccessful
                .Where(successful => successful)
                .Subscribe(_ => EmailTextField.ResignFirstResponder())
                .DisposedBy(DisposeBag);

            ViewModel.Reset.Executing
                .Subscribe(ActivityIndicator.Rx().IsVisibleWithFade())
                .DisposedBy(DisposeBag);

            //Commands
            ResetPasswordButton.Rx()
                .BindAction(ViewModel.Reset)
                .DisposedBy(DisposeBag);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (viewInitialized) return;

            viewInitialized = true;
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

        private void prepareViews()
        {
            NavigationController.NavigationBarHidden = false;

            MessageLabel.Text = Resources.PasswordResetMessage;

            ResetPasswordButton.SetTitle(Resources.SendEmail, UIControlState.Normal);

            EmailTextField.BecomeFirstResponder();
            EmailTextField.Rx().ShouldReturn()
                .Subscribe(ViewModel.Reset.Inputs)
                .DisposedBy(DisposeBag);

            ActivityIndicator.StartSpinning();

            ErrorLabel.Hidden = true;
        }
    }
}
