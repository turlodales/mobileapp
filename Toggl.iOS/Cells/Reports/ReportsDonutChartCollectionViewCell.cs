using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Toggl.Core.UI.ViewModels.Reports;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public partial class ReportsDonutChartCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("ReportsDonutChartCollectionViewCell");
        public static readonly UINib Nib;
        public static readonly int Height = 307;

        private bool isLast = false;

        static ReportsDonutChartCollectionViewCell()
        {
            Nib = UINib.FromName("ReportsDonutChartCollectionViewCell", NSBundle.MainBundle);
        }

        protected ReportsDonutChartCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        override public void AwakeFromNib()
        {
            base.AwakeFromNib();

            Layer.MasksToBounds = false;
            ContentView.ClipsToBounds = true;
        }

        public void SetElement(ReportDonutChartDonutElement element, bool last)
        {
            DonutChartView.UpdateSegments(element.Segments);
            isLast = last;
            SetNeedsLayout();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Layer.CornerRadius = 0;
            Layer.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;
            if (TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular)
            {
                Layer.CornerRadius = 8;
                if (isLast)
                {
                    Layer.MaskedCorners = (CACornerMask) 15;
                }
            }
        }
    }
}

