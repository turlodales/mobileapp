using System;
using System.Reactive.Linq;
using AuthenticationServices;
using Foundation;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.iOS.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class SsoLoginViewController : KeyboardAwareViewController<SsoViewModel>
    {
        public SsoLoginViewController(SsoViewModel vm) : base(vm, nameof(SsoLoginViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var closeButton = new UIBarButtonItem(
                UIImage.FromBundle("icClose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            closeButton.TintColor = ColorAssets.IconTint;

            var backButton = new UIBarButtonItem("",
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            backButton.TintColor = ColorAssets.IconTint;

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

            //Errors
            ViewModel.EmailErrorMessage
                .Subscribe(EmailErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.ErrorMessage
                .Subscribe(ErrorLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ContinueButton.Rx()
                .BindAction(ViewModel.Continue)
                .DisposedBy(DisposeBag);

            //Loading: disabling all interaction
            ViewModel.IsLoading
                .Select(CommonFunctions.Invert)
                .Subscribe(ContinueButton.Rx().Enabled())
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
                .Subscribe(EmailTextField.Rx().Enabled())
                .DisposedBy(DisposeBag);

            //Loading: making everything look disabled
            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(LogoImageView.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(ContinueButton.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(SsoHeaderLabel.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(opacityForLoadingState)
                .Subscribe(EmailTextField.Rx().AnimatedAlpha())
                .DisposedBy(DisposeBag);

            EmailTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            var keyboardHeight = e.FrameEnd.Height;
            ScrollView.ContentInset = new UIEdgeInsets(0, 0, keyboardHeight, 0);

            var firstResponder = View.GetFirstResponder();
            if (firstResponder != null && ScrollView.Frame.Height - keyboardHeight < ScrollView.ContentSize.Height)
            { 
                var scrollOffset = firstResponder.Frame.Y - (ScrollView.Frame.Height - keyboardHeight) / 2;
                ScrollView.ContentOffset = new CoreGraphics.CGPoint(0, scrollOffset);
            }
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            ScrollView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            ScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, 0), true);
        }

        private float opacityForLoadingState(bool isLoading)
            => isLoading ? 0.6f : 1;

        private void prepareViews()
        {
            SsoHeaderLabel.Text = Resources.SingleSignOn;
            EmailTextField.Placeholder = Resources.WorkEmail;
            ContinueButton.SetTitle(Resources.Continue, UIControlState.Normal);

            EmailTextField.ShouldReturn += _ =>
            {
                ViewModel.Continue.Execute();
                return false;
            };
        }
    }
}

