using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.iOS.Views
{
    [Register(nameof(TabBarIndicator))]
    public sealed class TabBarIndicator : UIView
    {
        private const int diameter = 8;
        private const int radius = diameter / 2;
        private const float animationDuration = 1.2f;
        private const float pauseBetweenAnimationRepeats = 0.8f;

        private readonly CAShapeLayer animationLayer = new CAShapeLayer();

        public override UIColor? BackgroundColor
        {
            get => base.BackgroundColor;
            set
            {
                base.BackgroundColor = value;
                animationLayer.FillColor = value?.CGColor;
            }
        }

        public TabBarIndicator(CGPoint center)
            : base(new CGRect(center.X - radius, center.Y - radius, diameter, diameter))
        {
        }

        public override void WillMoveToWindow(UIWindow? window)
        {
            base.WillMoveToWindow(window);

            setupAnimationLayer();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            animationLayer.Frame = Layer.Bounds;
            animationLayer.Path = UIBezierPath.FromOval(Bounds).CGPath;
        }

        private void setupAnimationLayer()
        {
            animationLayer.FillColor = BackgroundColor?.CGColor;
            animationLayer.CornerRadius = radius;
            Layer.CornerRadius = radius;
            Layer.AddSublayer(animationLayer);

            var scaleAnimation = CABasicAnimation.FromKeyPath("transform.scale");
            scaleAnimation.From = new NSNumber(1);
            scaleAnimation.To = new NSNumber(3);
            scaleAnimation.Duration = animationDuration;

            var opacityAnimation = CABasicAnimation.FromKeyPath("opacity");
            opacityAnimation.From = new NSNumber(0.8f);
            opacityAnimation.To = new NSNumber(0);
            opacityAnimation.Duration = animationDuration;

            var animationGroup = new CAAnimationGroup();
            animationGroup.Animations = new[] { scaleAnimation, opacityAnimation };
            animationGroup.RepeatCount = float.PositiveInfinity;
            animationGroup.Duration = animationDuration + pauseBetweenAnimationRepeats;

            animationLayer.AddAnimation(animationGroup, null);
        }
    }
}
