using System;
using Foundation;
using UIKit;

namespace Toggl.iOS.Extensions
{
    public static class LabelExtensions
    {
        public static void SetKerning(this UILabel label, double letterSpacing)
        {
            var text = label.Text ?? "";
            var attributedText = new NSMutableAttributedString(text);
            var range = new NSRange(0, text.Length);

            attributedText.AddAttribute(UIStringAttributeKey.KerningAdjustment, new NSNumber(letterSpacing), range);

            label.AttributedText = attributedText;
        }

        public static void SetTextWithOnboardingAppearance(this UILabel label, string message, bool isHeader = true, bool useInverseColor = true)
        {
            var paragraphStyle = new NSMutableParagraphStyle
            {
                MaximumLineHeight = isHeader ? 35 : 21,
                MinimumLineHeight = isHeader ? 35 : 21
            };

            var attributes = new UIStringAttributes
            {
                ForegroundColor = useInverseColor ? ColorAssets.InverseText : ColorAssets.Text,
                Font = UIFont.SystemFontOfSize(isHeader  ? 25 : 15),
                ParagraphStyle = paragraphStyle
            };

            var messageAttributedString = new NSMutableAttributedString(message);
            messageAttributedString.AddAttributes(attributes, new NSRange(0, message.Length));
            label.AttributedText = messageAttributedString;
        }
    }
}
