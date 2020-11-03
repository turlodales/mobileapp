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
                SetNeedsDisplay();
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
                SetNeedsDisplay();
            }
        }

        private CGPoint point1
        {
            get
            {
                switch (Direction)
                {
                    case TriangleDirection.Up:
                        return new CGPoint(0, Bounds.GetMaxY());
                    case TriangleDirection.Down:
                        return new CGPoint(0, 0);
                    case TriangleDirection.Left:
                        return new CGPoint(0, Bounds.GetMaxY() / 2);
                    case TriangleDirection.Right:
                        return new CGPoint(0, Bounds.GetMaxY());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private CGPoint point2
        {
            get
            {
                switch (Direction)
                {
                    case TriangleDirection.Up:
                        return new CGPoint(Bounds.GetMaxX() / 2, 0);
                    case TriangleDirection.Down:
                        return new CGPoint(Bounds.GetMaxX(), 0);
                    case TriangleDirection.Left:
                        return new CGPoint(Bounds.GetMaxX(), 0);
                    case TriangleDirection.Right:
                        return new CGPoint(0, 0);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private CGPoint point3
        {
            get
            {
                switch (Direction)
                {
                    case TriangleDirection.Up:
                        return new CGPoint(Bounds.GetMaxX(), Bounds.GetMaxY());
                    case TriangleDirection.Down:
                        return new CGPoint(Bounds.GetMaxX() / 2, Bounds.GetMaxY());
                    case TriangleDirection.Left:
                        return new CGPoint(Bounds.GetMaxX(), Bounds.GetMaxY());
                    case TriangleDirection.Right:
                        return new CGPoint(Bounds.GetMaxX(), Bounds.GetMaxY() / 2);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public TriangleView() : base() { }
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
            Down,
            Left,
            Right
        }
    }
}
