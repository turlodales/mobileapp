using System;
using CoreAnimation;
using Foundation;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public sealed partial class ReportsDonutChartPlaceholderCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ReportsDonutChartPlaceholderCollectionViewCell));
        public static readonly UINib Nib;

        private bool donutChartLegendVisible;

        public bool DonutChartLegendVisible
        {
            get => donutChartLegendVisible;
            set
            {
                donutChartLegendVisible = value;
                SetNeedsLayout();
                LayoutIfNeeded();
            }
        }

        static ReportsDonutChartPlaceholderCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(ReportsDonutChartPlaceholderCollectionViewCell), NSBundle.MainBundle);
        }

        protected ReportsDonutChartPlaceholderCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Layer.MasksToBounds = false;
            ContentView.ClipsToBounds = true;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Layer.CornerRadius = 0;
            Layer.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;
            if (TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular)
            {
                Layer.MasksToBounds = false;
                Layer.CornerRadius = 8;
                if (!DonutChartLegendVisible)
                {
                    Layer.MaskedCorners = (CACornerMask) 15;
                }
            }
        }
    }
}
