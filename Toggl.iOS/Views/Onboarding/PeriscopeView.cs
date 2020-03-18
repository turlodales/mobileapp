using System;
using CoreAnimation;
using CoreGraphics;
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
        }

        public override void LayoutSubviews()
        {
            // The magic numbers used here are to position the eyeball and pupil relative to the periscope image size
            base.LayoutSubviews();
            periscopeLayer.Frame = Bounds;
            eyeballLayer.Frame = new CGRect(Bounds.Width * 0.611, Bounds.Height * 0.061, Bounds.Width * 0.203, Bounds.Height * 0.221);
            pupilLayer.Frame = new CGRect(Bounds.Width * 0.770, Bounds.Height * 0.135, Bounds.Width * 0.110, Bounds.Height * 0.109);

            periscopeLayer.RemoveAllAnimations();

            var path = new UIBezierPath();
            path.MoveTo(new CGPoint(Bounds.Width * 0.773, Bounds.Height * 0.176));
            path.AddLineTo(new CGPoint(Bounds.Width * 0.676, Bounds.Height * 0.176));
            path.AddArc(new CGPoint(Bounds.Width * 0.725, Bounds.Height * 0.176), (nfloat)(Bounds.Width * 0.049), 0, (nfloat)Math.PI, true);
            path.ClosePath();

            var animation = new CAKeyFrameAnimation();
            animation.KeyPath = "position";
            animation.Duration = 3;
            animation.Path = path.CGPath;
            animation.AutoReverses = true;
            animation.RepeatCount = float.MaxValue;

            pupilLayer.AddAnimation(animation, "animation");
        }
    }
}
