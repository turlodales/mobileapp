using System.Text.RegularExpressions;

namespace Toggl.Core.Helper
{
    public static class Colors
    {
        private const string pattern = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
        private static readonly Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        public const string NoProject = "#B5BCC0";
        public const string ClientNameColor = "#FF5E5B5B";

        public static bool IsValidHexColor(string hex)
        {
            if (hex == null)
            {
                return false;
            }
            return regex.Match(hex).Length > 0;
        }

        public static readonly string[] DefaultProjectColors =
        {
            "#0B83D9", "#9E5BD9", "#D94182", "#E36A00", "#BF7000",
            "#C7AF14", "#D92B2B", "#2DA608", "#06A893", "#C9806B",
            "#465BB3", "#990099", "#566614", "#525266"
        };
    }
}
