using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.iOS.Views
{
    [Register(nameof(TriangleView))]
    public sealed class TriangleView : UIView
    {
        private TriangleDirection direction;
        public TriangleDirection Direction
        {
            get => direction;
            set
            {
                if (value == direction)
                    return;
                direction = value;
                Draw(Bounds);
            }
        }

        private UIColor color;
        public UIColor Color
        {
            get => color;
            set
            {
                if (value == color)
                    return;
                color = value;
                Draw(Bounds);
            }
        }

        private CGPoint point1 =>
            new CGPoint(
                0,
                Direction == TriangleDirection.Up ? Bounds.GetMaxY() : 0);

        private CGPoint point2 =>
            new CGPoint(
                Bounds.GetMaxX(),
                Direction == TriangleDirection.Up ? Bounds.GetMaxY() : 0);

        private CGPoint point3 =>
            new CGPoint(
                Bounds.GetMaxX() / 2,
                Direction == TriangleDirection.Up ? 0 : Bounds.GetMaxY());


        public TriangleView(CGRect frame) : base(frame) { }

        public TriangleView(IntPtr handle) : base(handle) { }

        public TriangleView(NSCoder coder) : base(coder) { }

        public TriangleView(NSObjectFlag t) : base(t) { }

        public override void Draw(CGRect rect)
        {
            if (color == null) return;
            var context = UIGraphics.GetCurrentContext();
            if (context == null) return;

            context.BeginPath();
            context.AddLines(new[] { point1, point2, point3 });
            context.ClosePath();

            context.SetFillColor(Color.CGColor);
            context.FillPath();
        }

        public enum TriangleDirection
        {
            Up,
            Down
        }
    }
}
