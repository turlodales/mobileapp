using System;
using CoreGraphics;
using Foundation;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Extensions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.Cells.Reports
{
    public partial class ReportsSummaryCollectionViewCell : UICollectionViewCell
    {
        private const int fontSize = 24;

        private static readonly UIColor percentageEnabledColor = Colors.Reports.PercentageActivated.ToNativeColor();
        private static readonly UIColor totalTimeEnabledColor = Colors.Reports.TotalTimeActivated.ToNativeColor();
        private static readonly UIColor disabledColor = Colors.Reports.Disabled.ToNativeColor();

        private static readonly int secondsPartLength = 3;
        private readonly UIStringAttributes durationEmphasizedAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = totalTimeEnabledColor
        };

        private readonly UIStringAttributes durationSecondsAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Light),
            ForegroundColor = totalTimeEnabledColor
        };

        private readonly UIStringAttributes normalAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = percentageEnabledColor
        };

        private readonly UIStringAttributes disabledAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = disabledColor
        };

        public static readonly NSString Key = new NSString("ReportsSummaryCollectionViewCell");
        public static readonly UINib Nib;
        public static readonly int Height = 84;

        static ReportsSummaryCollectionViewCell()
        {
            Nib = UINib.FromName("ReportsSummaryCollectionViewCell", NSBundle.MainBundle);
        }

        protected ReportsSummaryCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        override public void AwakeFromNib()
        {
            base.AwakeFromNib();

            TotalTimeTitleLabel.Text = Resources.Total.ToUpper();
            BillableTitleLabel.Text = Resources.Billable.ToUpper();

            ContentView.Layer.MasksToBounds = true;
            ContentView.Layer.CornerRadius = 8;
            Layer.MasksToBounds = false;
            Layer.CornerRadius = 8;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowRadius = 8;
            Layer.ShadowOffset = new CGSize(0, 2);
            Layer.ShadowOpacity = 0.1f;

            TotalTimeTitleLabel.SetKerning(-0.2);
            TotalTimeLabel.SetKerning(-0.2);
            BillableTitleLabel.SetKerning(-0.2);
            BillablePercentageLabel.SetKerning(-0.2);
        }

        public void SetElement(ReportSummaryElement element)
        {
            if (element.IsLoading)
            {
                LoadingView.Hidden = false;
            }
            else
            {
                LoadingView.Hidden = true;
                BillablePercentageLabel.AttributedText = billableFormattedString(element.BillablePercentage);
                TotalTimeLabel.AttributedText = durationFormattedString(element.TotalTime, element.DurationFormat);
                TotalTimeLabel.TextColor = element.TotalTime == TimeSpan.Zero
                    ? disabledColor
                    : totalTimeEnabledColor;
            }
        }

        private NSAttributedString billableFormattedString(float? value)
        {
            var isDisabled = value == null;
            var actualValue = isDisabled ? 0 : value.Value;

            var percentage = $"{actualValue:0.00}%";

            var attributes = isDisabled ? disabledAttributes : normalAttributes;
            return new NSAttributedString(percentage, attributes);
        }

        private NSAttributedString durationFormattedString(TimeSpan? duration, DurationFormat format)
        {
            var durationText = ((TimeSpan) duration).ToFormattedString(format);

            if (format == DurationFormat.Classic || format == DurationFormat.Decimal)
                return new NSAttributedString(durationText, durationEmphasizedAttributes);

            var emphazisedLength = durationText.Length - secondsPartLength;
            var attributedString = new NSMutableAttributedString(durationText);
            attributedString.AddAttributes(durationEmphasizedAttributes, new NSRange(0, emphazisedLength));
            attributedString.AddAttributes(durationSecondsAttributes, new NSRange(emphazisedLength, secondsPartLength));
            return attributedString;
        }
    }
}
