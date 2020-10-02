using System;
using CoreGraphics;
using Foundation;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public partial class ReportsBarChartPlaceholderCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ReportsBarChartPlaceholderCollectionViewCell));
        public static readonly UINib Nib;
        public static readonly int Height = 270;

        static ReportsBarChartPlaceholderCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(ReportsBarChartPlaceholderCollectionViewCell), NSBundle.MainBundle);
        }

        protected ReportsBarChartPlaceholderCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            YouHaventTrackedAnythingLabel.Text = Resources.YouHaventTrackedAnythingYet;
            GetTrackingLabel.Text = Resources.GetTracking;

            ContentView.Layer.MasksToBounds = true;
            ContentView.Layer.CornerRadius = 8;
            Layer.MasksToBounds = false;
            Layer.CornerRadius = 8;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowRadius = 8;
            Layer.ShadowOffset = new CGSize(0, 2);
            Layer.ShadowOpacity = 0.1f;
        }
    }
}

