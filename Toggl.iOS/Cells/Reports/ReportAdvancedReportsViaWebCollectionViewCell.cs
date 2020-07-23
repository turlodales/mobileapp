using System;
using CoreGraphics;
using Foundation;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Extensions;
using Toggl.iOS.Shared;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public sealed partial class ReportAdvancedReportsViaWebCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ReportAdvancedReportsViaWebCollectionViewCell));
        public static readonly UINib Nib;

        public const int Height = 198;

        static ReportAdvancedReportsViaWebCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(ReportAdvancedReportsViaWebCollectionViewCell), NSBundle.MainBundle);
        }

        protected ReportAdvancedReportsViaWebCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            TitleLabel.Text = Resources.AdvancedReportsViaWeb.ToUpper();
            MessageLabel.Text = Resources.AdvancedReportsFeatures;
            ButtonLabel.Text = Resources.AvailableOnOtherPlans;

            MessageLabel.SetLineSpacing(6, UITextAlignment.Left);
            ArrowImageView.SetTemplateColor(ColorAssets.ReportsBarChartFilled);

            ContentView.Layer.MasksToBounds = true;
            ContentView.Layer.CornerRadius = 8;
            Layer.MasksToBounds = false;
            Layer.CornerRadius = 8;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowRadius = 8;
            Layer.ShadowOffset = new CGSize(0, 2);
            Layer.ShadowOpacity = 0.1f;
        }

        public void SetElement(ReportAdvancedReportsViaWebElement element)
        {
            ButtonLabel.Hidden = !element.ShouldShowAvailableOnOtherPlans;
            ArrowImageView.Hidden = !element.ShouldShowAvailableOnOtherPlans;
        }
    }
}

