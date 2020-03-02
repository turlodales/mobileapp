using System;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using ObjCRuntime;
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
        private AVPlayer player;
        private AVPlayerLayer playerLayer;

        public OnboardingViewController(OnboardingViewModel viewModel) : base(viewModel, nameof(OnboardingViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, restartVideo);

            configureVideo();
            configureMessageAppearance();
            configureButtonsAppearance();

            ContinueWithEmailButton.Rx()
                .BindAction(ViewModel.ContinueWithEmail)
                .DisposedBy(DisposeBag);

            ContinueWithGoogleButton.Rx().Tap()
                .Subscribe(ViewModel.ContinueWithGoogle.Inputs)
                .DisposedBy(DisposeBag);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            playerLayer.Frame = TogglmanView.Bounds;
            restartVideo(null);
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            // Update shadows and borders in the buttons
            // this is needed because CGColor doesn't know about the current trait collection
            configureButtonsAppearance();
        }

        private void configureVideo()
        {
            var url = NSBundle.MainBundle.GetUrlForResource("togglman", "mp4");
            player = AVPlayer.FromUrl(url);
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, restartVideo, player.CurrentItem);

            playerLayer = AVPlayerLayer.FromPlayer(player);
            TogglmanView.Layer.AddSublayer(playerLayer);

            playerLayer.Frame = TogglmanView.Bounds;
            restartVideo(null);
        }

        private void restartVideo(NSNotification notification)
        {
            player.Seek(CMTime.Zero);
            player.Play();
        }

        private void configureMessageAppearance()
        {
            var message = Resources.OnboardingMessageVariantA;
            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = (nfloat)1.18;

            var attributes = new UIStringAttributes();
            attributes.ForegroundColor = ColorAssets.InverseText;
            attributes.Font = UIFont.SystemFontOfSize(28);
            attributes.ParagraphStyle = paragraphStyle;

            var messageAttributedString = new NSMutableAttributedString(message);
            messageAttributedString.AddAttributes(attributes, new NSRange(0, message.Length));
            MessageLabel.AttributedText = messageAttributedString;
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
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(this, AVPlayerItem.DidPlayToEndTimeNotification, player.CurrentItem);
            }
        }
    }
}
