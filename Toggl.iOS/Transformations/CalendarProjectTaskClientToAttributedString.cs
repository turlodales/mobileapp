using System;
using System.Text;
using Foundation;
using Toggl.Core.Calendar;
using Toggl.Core.UI.Helper;
using Toggl.iOS.Extensions;
using Toggl.iOS.Helper;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.Transformations
{
    public class CalendarProjectTaskClientToAttributedString
    {
        private readonly nfloat fontHeight;

        public CalendarProjectTaskClientToAttributedString(nfloat fontHeight)
        {
            this.fontHeight = fontHeight;
        }

        public NSAttributedString Convert(CalendarItem calendarItem)
        {
            var builder = new StringBuilder();
            var hasClient = !string.IsNullOrEmpty(calendarItem.Client);

            if (!string.IsNullOrEmpty(calendarItem.Project))
                builder.Append($"{calendarItem.Project}");

            if (!string.IsNullOrEmpty(calendarItem.Task))
                builder.Append($": {calendarItem.Task}");

            if (hasClient)
                builder.Append($"· {calendarItem.Client}");

            var text = builder.ToString();

            var result = new NSMutableAttributedString(text);
            var clientIndex = text.Length - (calendarItem.Client?.Length ?? 0);

            var projectColor = Color
                .ParseAndAdjustToLabel(calendarItem.Color, ActiveTheme.Is.DarkTheme)
                .ToNativeColor();

            var projectNameRange = new NSRange(0, clientIndex);
            var projectNameAttributes = new UIStringAttributes { ForegroundColor = projectColor };
            result.AddAttributes(projectNameAttributes, projectNameRange);

            if (hasClient)
            {
                var clientColor = Colors.TimeEntriesLog.ClientColor.ToNativeColor();
                var clientNameRange = new NSRange(clientIndex, calendarItem.Client.Length);
                var clientNameAttributes = new UIStringAttributes {ForegroundColor = clientColor};
                result.AddAttributes(clientNameAttributes, clientNameRange);
            }

            return result;
        }
    }
}
