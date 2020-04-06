using System;
using AuthenticationServices;
using CoreGraphics;
using Foundation;
using Toggl.Core.Analytics;
using Toggl.Core.UI.Models;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public sealed partial class OnboardingViewController : ReactiveViewController<OnboardingViewModel>
    {
        private UIView containerView;
        private OnboardingPageView page1;
        private OnboardingPageView page2;
        private OnboardingPageView page3;

        private const double duration = 0.3;
        private UISwipeGestureRecognizer swipeLeftGesture;
        private UISwipeGestureRecognizer swipeRightGesture;

        private const int totalPages = 3;
        private int currentPage = 0;

        private OnboardingLoadingView loadingView;

        private ASAuthorizationAppleIdButton appleSignInButton;
        private IDisposable appleSignInButtonDisposable;

        public OnboardingViewController(OnboardingViewModel viewModel) : base(viewModel, nameof(OnboardingViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            loadingView = new OnboardingLoadingView();
            loadingView.TranslatesAutoresizingMaskIntoConstraints = false;

            configureButtonsAppearance();

            ContinueWithEmailButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithEmail.Inputs)
                .DisposedBy(DisposeBag);

            ContinueWithGoogleButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithGoogle.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Subscribe(toggleLoadingView)
                .DisposedBy(DisposeBag);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            configureOnboardingPages();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            page1.Frame = View.Bounds;
            page2.Frame = View.Bounds;
            page3.Frame = View.Bounds;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            page1.RemoveFromSuperview();
            page2.RemoveFromSuperview();
            page3.RemoveFromSuperview();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            // Update shadows and borders in the buttons
            // this is needed because CGColor doesn't know about the current trait collection
            configureButtonsAppearance();
        }

        private void configureOnboardingPages()
        {
            PageControl.CurrentPageIndicatorTintColor = ColorAssets.Background;
            PageControl.PageIndicatorTintColor = ColorAssets.Background.ColorWithAlpha((nfloat)0.5);

            swipeLeftGesture = new UISwipeGestureRecognizer(moveToNextPage);
            swipeLeftGesture.Direction = UISwipeGestureRecognizerDirection.Left;

            swipeRightGesture = new UISwipeGestureRecognizer(moveToPreviousPage);
            swipeRightGesture.Direction = UISwipeGestureRecognizerDirection.Right;

            containerView = new UIView();
            containerView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.InsertSubview(containerView, 0);
            containerView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            containerView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            containerView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            containerView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            page1 = OnboardingPageView.Create();
            page1.BackgroundColor = ColorAssets.OnboardingPage1BackgroundColor;
            page1.SetVideo(NSBundle.MainBundle.GetUrlForResource("togglman", "mp4"));
            page1.Message = Resources.OnboardingMessagePage1;

            page2 = OnboardingPageView.Create();
            page2.BackgroundColor = ColorAssets.OnboardingPage2BackgroundColor;
            page2.SetContentView(new PeriscopeView());
            page2.Message = Resources.OnboardingMessagePage2;

            page3 = OnboardingPageView.Create();
            page3.BackgroundColor = ColorAssets.OnboardingPage3BackgroundColor;
            page3.SetImageView(UIImage.FromBundle("ic_hand"));
            page3.Message = Resources.OnboardingMessagePage3;

            containerView.InsertSubview(page3, 0);
            containerView.InsertSubview(page2, 1);
            containerView.InsertSubview(page1, 2);

            View.AddGestureRecognizer(swipeLeftGesture);
            View.AddGestureRecognizer(swipeRightGesture);

            ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
            {
                Action = OnboardingScrollAction.Automatic,
                Direction = OnboardingScrollDirection.Left,
                PageNumber = currentPage,
            });
        }

        private void configureButtonsAppearance()
        {
            // Continue with email
            ContinueWithEmailButton.TitleLabel.Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium);
            ContinueWithEmailButton.SetTitle(Resources.ContinueWithEmail, UIControlState.Normal);
            ContinueWithEmailButton.SetTitleColor(ColorAssets.InverseText, UIControlState.Normal);
            ContinueWithEmailButton.Layer.CornerRadius = 8;
            ContinueWithEmailButton.Layer.BorderWidth = 1;
            ContinueWithEmailButton.Layer.BorderColor = ColorAssets.InverseText.CGColor;

            // Continue with google
            ContinueWithGoogleButton.TitleLabel.Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium);
            ContinueWithGoogleButton.SetTitle(Resources.ContinueWithGoogle, UIControlState.Normal);
            ContinueWithGoogleButton.SetTitleColor(ColorAssets.Text, UIControlState.Normal);
            ContinueWithGoogleButton.Layer.MasksToBounds = false;
            ContinueWithGoogleButton.Layer.CornerRadius = 8;
            ContinueWithGoogleButton.Layer.ShadowColor = UIColor.Black.CGColor;
            ContinueWithGoogleButton.Layer.ShadowOpacity = (float)0.15;
            ContinueWithGoogleButton.Layer.ShadowRadius = 6;
            ContinueWithGoogleButton.Layer.ShadowOffset = new CGSize(0, 2);

            // Continue with apple
            configureSignInWithApple();
        }

        private void configureSignInWithApple()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                return;

            if (appleSignInButton != null)
            {
                appleSignInButtonDisposable?.Dispose();
                appleSignInButtonDisposable = null;
                ButtonsStackView.RemoveArrangedSubview(appleSignInButton);
                appleSignInButton = null;
            }

            var style = TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Light
                ? ASAuthorizationAppleIdButtonStyle.White
                : ASAuthorizationAppleIdButtonStyle.Black;
            appleSignInButton = new ASAuthorizationAppleIdButton(ASAuthorizationAppleIdButtonType.Continue, style);
            ButtonsStackView.InsertArrangedSubview(appleSignInButton, 0);

            appleSignInButtonDisposable = appleSignInButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithApple.Inputs);
        }

        private void moveToNextPage(UISwipeGestureRecognizer swipe)
        {
            var next = nextPageView();
            var frame = View.Bounds;
            frame.Offset(View.Bounds.Width, 0);
            next.Frame = frame;
            containerView.BringSubviewToFront(next);

            UIView.Animate(
                duration,
                () => { animatePage(next); },
                () => { moveIndicatorToNextPage(OnboardingScrollAction.Manual); });
        }

        private void moveToPreviousPage(UISwipeGestureRecognizer swipe)
        {
            var previous = previousPageView();
            var frame = View.Bounds;
            frame.Offset(-View.Bounds.Width, 0);
            previous.Frame = frame;
            containerView.BringSubviewToFront(previous);

            UIView.Animate(
                duration,
                () => { animatePage(previous); },
                () => { moveIndicatorToPreviousPage(OnboardingScrollAction.Manual); });
        }

        private void animatePage(OnboardingPageView page)
        {
            page.Frame = View.Bounds;
        }

        private void moveIndicatorToNextPage(OnboardingScrollAction action)
        {
            currentPage = currentPage < totalPages - 1
                ? currentPage + 1
                : 0;
            PageControl.CurrentPage = currentPage;
            ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
            {
                Action = action,
                Direction = OnboardingScrollDirection.Right,
                PageNumber = currentPage,
            });
        }

        private void moveIndicatorToPreviousPage(OnboardingScrollAction action)
        {
            currentPage = currentPage > 0
                ? currentPage - 1
                : totalPages - 1;
            PageControl.CurrentPage = currentPage;
            ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
            {
                Action = action,
                Direction = OnboardingScrollDirection.Left,
                PageNumber = currentPage,
            });
        }

        private OnboardingPageView nextPageView()
        {
            switch (currentPage)
            {
                case 0: return page2;
                case 1: return page3;
                case 2: return page1;
            }

            throw new IndexOutOfRangeException();
        }

        private OnboardingPageView previousPageView()
        {
            switch (currentPage)
            {
                case 0: return page3;
                case 1: return page1;
                case 2: return page2;
            }

            throw new IndexOutOfRangeException();
        }

        private void toggleLoadingView(bool isLoading)
        {
            if (!isLoading)
            {
                loadingView.RemoveFromSuperview();
            }
            else
            {
                View.AddSubview(loadingView);
                loadingView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
                loadingView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
                loadingView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
                loadingView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            }
        }
    }
}
