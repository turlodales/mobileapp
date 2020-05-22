using Toggl.Droid.Extensions;
using Toggl.Droid.Helper;
using Toggl.Shared;
using Color = Android.Graphics.Color;

namespace Toggl.Droid.Autocomplete
{
    public sealed class ProjectTokenSpan : TokenSpan
    {
        public long ProjectId { get; }

        public string ProjectName { get; }

        public string ProjectColor { get; }

        public long? TaskId { get; set; }

        public string TaskName { get; set; }

        public ProjectTokenSpan(long projectId, string projectName, string projectColor, long? taskId, string taskName)
            : base(
                  Shared.Color.ParseAndAdjustToUserTheme(projectColor, ActiveTheme.Is.DarkTheme).ToNativeColor(),
                  Color.White,
                  false
            )
        {
            ProjectId = projectId;
            ProjectName = projectName;
            ProjectColor = projectColor;
            TaskId = taskId;
            TaskName = taskName ?? string.Empty;
        }
    }
}
