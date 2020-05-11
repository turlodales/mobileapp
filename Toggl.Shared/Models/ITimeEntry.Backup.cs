using System;
using System.Collections.Generic;

namespace Toggl.Shared.Models
{
    public partial interface ITimeEntry
    {
        bool ContainsBackup { get; set; }
        long? ProjectIdBackup { get; set; }
        long? TaskIdBackup { get; set; }
        bool BillableBackup { get; set; }
        DateTimeOffset StartBackup { get; set; }
        long? DurationBackup { get; set; }
        string DescriptionBackup { get; set; }
        IList<long> TagIdsBackup { get; }
    }
}
