using Toggl.Core.Calendar;
using Toggl.Core.UI.Helper;
using Toggl.Shared;
using static System.Math;

namespace Toggl.Core.UI.Extensions
{
    public static class ColorExtensions
    {
        // Taken from http://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs,23adaaa39209cc1f
        public static (float hue, float saturation, float value) GetHSV(this Color color)
        {
            var max = Max(color.Red, Max(color.Green, color.Blue));
            var min = Min(color.Red, Min(color.Green, color.Blue));

            return (
                hue: color.GetHue() / 360,
                saturation: (float)((max <= 0) ? 0 : 1d - (1d * min / max)),
                value: (float)(max / 255d));
        }

        public static Color ForegroundColor(this CalendarItem calendarItem)
        {
            var color = new Color(calendarItem.Color);
            var luma = color.CalculateLuminance();
            return luma < 0.5 ? Colors.White : Colors.Black;
        }
    }
}
