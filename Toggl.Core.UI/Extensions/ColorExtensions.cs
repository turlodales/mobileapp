using Toggl.Core.Calendar;
using Toggl.Core.UI.Helper;
using Toggl.Shared;
using static System.Math;

namespace Toggl.Core.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ForegroundColor(this CalendarItem calendarItem)
        {
            var color = new Color(calendarItem.Color);
            var luma = color.CalculateLuminance();
            return luma < 0.5 ? Colors.White : Colors.Black;
        }
    }
}
