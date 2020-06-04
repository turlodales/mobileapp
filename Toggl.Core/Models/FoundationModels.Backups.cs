using System;
using System.Collections.Generic;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage;
using Toggl.Storage.Models;

namespace Toggl.Core.Models
{
    internal partial class TimeEntry : IThreadSafeTimeEntry
    {
        public bool ContainsBackup { get; set; }

        public long? ProjectIdBackup { get; set; }

        public long? TaskIdBackup { get; set; }

        public bool BillableBackup { get; set; }

        public DateTimeOffset StartBackup { get; set; }

        public long? DurationBackup { get; set; }

        public string DescriptionBackup { get; set; }

        public IList<long> TagIdsBackup { get; } = new List<long>();
    }

    internal partial class User : IThreadSafeUser
    {
        public bool ContainsBackup { get; set; }

        public long? DefaultWorkspaceIdBackup { get; set; }

        public BeginningOfWeek BeginningOfWeekBackup { get; set; }
    }

    internal partial class Preferences : IThreadSafePreferences
    {
        public bool ContainsBackup { get; set; }

        public TimeFormat TimeOfDayFormatBackup { get; set; }

        public DateFormat DateFormatBackup { get; set; }

        public DurationFormat DurationFormatBackup { get; set; }

        public bool CollapseTimeEntriesBackup { get; set; }
    }
}
