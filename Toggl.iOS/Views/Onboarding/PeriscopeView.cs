using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.iOS
{
    public sealed class PeriscopeView : UIView
    {
        private readonly CALayer periscopeLayer;
        private readonly CALayer eyeballLayer;
        private readonly CALayer pupilLayer;

        public PeriscopeView()
        {
            periscopeLayer = new CALayer();
            eyeballLayer = new CALayer();
            pupilLayer = new CALayer();

            periscopeLayer.Contents = UIImage.FromBundle("ic_periscope").CGImage;
            eyeballLayer.Contents = UIImage.FromBundle("ic_eyeball").CGImage;
            pupilLayer.Contents = UIImage.FromBundle("ic_pupil").CGImage;

            Layer.AddSublayer(eyeballLayer);
            Layer.AddSublayer(pupilLayer);
            Layer.AddSublayer(periscopeLayer);

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, restartAnimation);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            restartAnimation(null);
        }

        public void restartAnimation(NSNotification notification)
        {
            // The magic numbers used here are to position the eyeball and pupil relative to the periscope image size
            periscopeLayer.Frame = Bounds;
            eyeballLayer.Frame = new CGRect(Bounds.Width * 0.571, Bounds.Height * 0.123, Bounds.Width * 0.203, Bounds.Height * 0.221);
            pupilLayer.Frame = new CGRect(Bounds.Width * 0.730, Bounds.Height * 0.197, Bounds.Width * 0.110, Bounds.Height * 0.109);

            pupilLayer.RemoveAllAnimations();

            var path = new UIBezierPath();
            path.MoveTo(new CGPoint(Bounds.Width * 0.725, Bounds.Height * 0.238));
            path.AddLineTo(new CGPoint(Bounds.Width * 0.628, Bounds.Height * 0.238));
            path.AddArc(new CGPoint(Bounds.Width * 0.677, Bounds.Height * 0.238), (nfloat)(Bounds.Width * 0.049), 0, (nfloat)Math.PI, true);
            path.ClosePath();

            var animation = new CAKeyFrameAnimation();
            animation.KeyPath = "position";
            animation.Duration = 5;
            animation.Path = path.CGPath;
            animation.AutoReverses = true;
            animation.RepeatCount = float.MaxValue;

            pupilLayer.AddAnimation(animation, "animation");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }
    }
}
