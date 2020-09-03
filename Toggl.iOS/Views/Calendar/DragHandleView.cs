using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Toggl.iOS.Extensions;
using UIKit;

namespace Toggl.iOS.Views.Calendar
{
    [Register("DragHandleView")]
    public sealed class DragHandleView : UIView
    {
        private CAShapeLayer topLine;
        private CAShapeLayer bottomLine;

        public DragHandleView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            BackgroundColor = UIColor.Clear;
            topLine = new CAShapeLayer();
            topLine.Path = CGPath.FromRect(new CGRect(0, 0, 10, 1));
            topLine.Frame = new CGRect(0, 1, 10, 1);
            Layer.AddSublayer(topLine);

            bottomLine = new CAShapeLayer();
            bottomLine.Path = CGPath.FromRect(new CGRect(0, 0, 10, 1));
            bottomLine.Frame = new CGRect(0, 3, 10, 1);
            Layer.AddSublayer(bottomLine);

            setColor();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            setColor();
        }

        private void setColor()
        {
            topLine.BackgroundColor = ColorAssets.CalendarDragHandles.CGColor;
            bottomLine.BackgroundColor = ColorAssets.CalendarDragHandles.CGColor;
        }
    }
}
