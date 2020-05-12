using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System.Threading;
using UIKit;

namespace Toggl.iOS
{
    public sealed class PeriscopeView : UIView
    {
        private readonly CALayer periscopeLayer;
        private readonly CALayer eyeballLayer;
        private readonly CALayer pupilLayer;
        private readonly CALayer eyelidLayer;
        private readonly CAShapeLayer eyelidMask;
        private NSObject observer = null;

        private Timer timer;

        public PeriscopeView()
        {
            periscopeLayer = new CALayer();
            eyeballLayer = new CALayer();
            pupilLayer = new CALayer();
            eyelidLayer = new CALayer();
            eyelidMask = new CAShapeLayer();

            periscopeLayer.Contents = UIImage.FromBundle("ic_periscope").CGImage;
            eyeballLayer.Contents = UIImage.FromBundle("ic_eyeball").CGImage;
            pupilLayer.Contents = UIImage.FromBundle("ic_pupil").CGImage;
            eyelidLayer.Contents = UIImage.FromBundle("ic_eyelid").CGImage;

            Layer.AddSublayer(eyeballLayer);
            Layer.AddSublayer(pupilLayer);
            Layer.AddSublayer(eyelidLayer);
            Layer.AddSublayer(periscopeLayer);

            observer = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, restartAnimation);
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
            eyelidLayer.Frame = new CGRect(Bounds.Width * 0.571, Bounds.Height * 0.123, Bounds.Width * 0.203, Bounds.Height * 0.221);
            pupilLayer.Frame = new CGRect(Bounds.Width * 0.730, Bounds.Height * 0.197, Bounds.Width * 0.110, Bounds.Height * 0.109);
            pupilLayer.Position = new CGPoint(eyeballLayer.Frame.GetMidX(), eyeballLayer.Frame.GetMidY());

            eyelidMask.FillRule = CAShapeLayer.FillRuleEvenOdd;
            eyelidMask.Path = CGPath.FromRect(eyelidLayer.Bounds);

            timer = new Timer(
                (state) =>
                    InvokeOnMainThread(() =>
                        movePupil()
                    ),
                null,
                0,
                1000);
        }

        private void movePupil()
        {
            var random = new Random();

            // move eye
            if (random.Next(0, 6) == 0)
            {
                var movementFrame = eyeballLayer.Frame;
                movementFrame.Inflate( -movementFrame.Width / 3, -movementFrame.Height / 3);

                var p = new CGPoint(
                    movementFrame.X - pupilLayer.Frame.Width / 2 + random.Next(0, (int) movementFrame.Width),
                    movementFrame.Y - pupilLayer.Frame.Height / 2 + random.Next(0, (int) movementFrame.Height)
                );

                pupilLayer.Frame = new CGRect(p, pupilLayer.Frame.Size);
            }

            // blink
            if (random.Next(0, 10) == 0)
            {
                eyelidLayer.Mask = eyelidMask;
                eyelidMask.RemoveAllAnimations();
                var initialPosition = eyelidMask.Position;

                var path = new UIBezierPath();
                path.MoveTo(new CGPoint(initialPosition.X, initialPosition.Y - eyelidLayer.Frame.Height));
                path.AddLineTo(initialPosition);
                path.AddLineTo(new CGPoint(initialPosition.X, initialPosition.Y - eyelidLayer.Frame.Height));
                path.ClosePath();

                var animation = new CAKeyFrameAnimation();
                animation.KeyPath = "position";
                animation.Duration = 0.3;
                animation.Path = path.CGPath;
                animation.RepeatCount = random.Next(0, 5) == 0 ? 2 : 0;
                animation.FillMode = CAFillMode.Forwards;
                animation.RemovedOnCompletion = false;

                eyelidMask.AddAnimation(animation, "animation");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            timer.Dispose();

            if (observer != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
                observer = null;
            }
        }
    }
}
