using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Extensions;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public partial class ReportsDonutChartLegendCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("ReportsDonutChartLegendCollectionViewCell");
        public static readonly UINib Nib;
        public static readonly int Height = 56;

        private UIView bottomSeparator;
        private bool isLast = false;

        static ReportsDonutChartLegendCollectionViewCell()
        {
            Nib = UINib.FromName("ReportsDonutChartLegendCollectionViewCell", NSBundle.MainBundle);
        }

        protected ReportsDonutChartLegendCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        override public void AwakeFromNib()
        {
            base.AwakeFromNib();

            FadeView.FadeRight = true;
            bottomSeparator = ContentView.InsertSeparator();
            ContentView.InsertSeparator(UIRectEdge.Top);

            Layer.MasksToBounds = false;
            ContentView.ClipsToBounds = true;

            ProjectLabel.SetKerning(-0.2);
            ClientLabel.SetKerning(-0.2);
            TotalTimeLabel.SetKerning(-0.2);
            PercentageLabel.SetKerning(-0.2);
        }

        public void SetElement(ReportProjectsDonutChartLegendItemElement element, bool last)
        {
            ProjectLabel.Text = element.Name;
            ClientLabel.Text = element.Client;
            PercentageLabel.Text = $"{element.Percentage:F2}%";
            TotalTimeLabel.Text = element.Value;

            ClientLabel.Hidden = element.Client == string.Empty;

            ProjectLabel.TextColor = Color.ParseAndAdjustToLabel(
                element.Color, ActiveTheme.Is.DarkTheme
            ).ToNativeColor();

            CircleView.BackgroundColor = Color.ParseAndAdjustToUserTheme(
                element.Color, ActiveTheme.Is.DarkTheme
            ).ToNativeColor(); ;

            isLast = last;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            bottomSeparator.Hidden = true;
            Layer.CornerRadius = 0;

            if (isLast)
            {
                if (TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular)
                {
                    Layer.CornerRadius = 8;
                    Layer.MaskedCorners = CACornerMask.MinXMaxYCorner | CACornerMask.MaxXMaxYCorner;
                    return;
                }
                bottomSeparator.Hidden = false;
            }
        }
    }
}

