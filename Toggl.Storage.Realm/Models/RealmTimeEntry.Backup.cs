using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Storage.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTimeEntry : RealmObject, IDatabaseTimeEntry
    {
        public bool HasBillableBackup { get; set; }
        public bool BillableBackup { get; set; }

        public bool HasStartBackup { get; set; }
        public DateTimeOffset StartBackup { get; set; }

        public bool HasDurationBackup { get; set; }
        public long? DurationBackup { get; set; }

        public bool HasDescriptionBackup { get; set; }
        public string DescriptionBackup { get; set; }

        public bool HasProjectIdBackup { get; set; }
        public long? ProjectIdBackup { get; set; }

        public bool HasTaskIdBackup { get; set; }
        public long? TaskIdBackup { get; set; }

        public bool HasTagIdsBackup { get; set; }
        public IList<long> TagIdsBackup { get; }
    }
}
