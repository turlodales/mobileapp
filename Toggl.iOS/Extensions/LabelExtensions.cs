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

        public static void SetTextWithOnboardingAppearance(this UILabel label, string message)
        {
            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.LineSpacing = (nfloat)1.18;

            var attributes = new UIStringAttributes();
            attributes.ForegroundColor = ColorAssets.InverseText;
            attributes.Font = UIFont.SystemFontOfSize(25);
            attributes.ParagraphStyle = paragraphStyle;

            var messageAttributedString = new NSMutableAttributedString(message);
            messageAttributedString.AddAttributes(attributes, new NSRange(0, message.Length));
            label.AttributedText = messageAttributedString;
        }
    }
}
