using System;
using System.Collections.Generic;

namespace Toggl.Shared.Models
{
    public partial interface ITimeEntry
    {
        long? ProjectIdBackup { get; set; }
        bool HasProjectIdBackup { get; set; }

        long? TaskIdBackup { get; set; }
        bool HasTaskIdBackup { get; set; }

        bool BillableBackup { get; set; }
        bool HasBillableBackup { get; set; }

        DateTimeOffset StartBackup { get; set; }
        bool HasStartBackup { get; set; }

        long? DurationBackup { get; set; }
        bool HasDurationBackup { get; set; }

        string DescriptionBackup { get; set; }
        bool HasDescriptionBackup { get; set; }

        IList<long> TagIdsBackup { get; }
        bool HasTagIdsBackup { get; set; }
    }
}
