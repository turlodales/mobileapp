using System;
using AVFoundation;
using CoreMedia;
using Foundation;
using UIKit;

namespace Toggl.iOS
{
    public sealed class VideoView : UIView
    {
        private readonly NSUrl url;
        private readonly AVPlayer player;
        private readonly AVPlayerLayer playerLayer;

        public VideoView(NSUrl url)
        {
            this.url = url;

            player = AVPlayer.FromUrl(url);
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, restartVideo, player.CurrentItem);

            playerLayer = AVPlayerLayer.FromPlayer(player);
            Layer.AddSublayer(playerLayer);

            playerLayer.Frame = Bounds;
            restartVideo(null);

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, restartVideo);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            playerLayer.Frame = Bounds;
        }

        public void RestartVideo()
        {
            restartVideo(null);
        }

        private void restartVideo(NSNotification notification)
        {
            player.Seek(CMTime.Zero);
            player.Play();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            player.Dispose();
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }
    }
}
