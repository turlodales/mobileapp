using Android.Text;

namespace Toggl.Droid.Extensions
{
    public static class StaticLayoutExtensions
    {
        public static int GetDesiredWidth(this StaticLayout textLayout)
            => (int)StaticLayout.GetDesiredWidth(textLayout.Text, 0, textLayout.Text.Length, textLayout.Paint);
    }
}
