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
        public bool ContainsBackup { get; set;  }

        public long? ProjectIdBackup { get; set; }

        public long? TaskIdBackup { get; set; }

        public bool BillableBackup { get; set; }

        public DateTimeOffset StartBackup { get; set; }

        public long? DurationBackup { get; set; }

        public string DescriptionBackup { get; set; }

        public IList<long> TagIdsBackup { get; set; }
    }
}
