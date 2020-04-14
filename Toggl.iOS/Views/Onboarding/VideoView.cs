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

        private NSObject didPlayToEndObserver = null;
        private NSObject didBecomeActiveObserver = null;

        public VideoView(NSUrl url)
        {
            this.url = url;

            player = AVPlayer.FromUrl(url);
            didPlayToEndObserver = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, restartVideo, player.CurrentItem);

            playerLayer = AVPlayerLayer.FromPlayer(player);
            Layer.AddSublayer(playerLayer);

            playerLayer.Frame = Bounds;
            restartVideo(null);

            didBecomeActiveObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, restartVideo);
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
            player.Dispose();

            if (didPlayToEndObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(didPlayToEndObserver);
                didPlayToEndObserver = null;
            }

            if (didBecomeActiveObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(didBecomeActiveObserver);
                didBecomeActiveObserver = null;
            }
        }
    }
}
