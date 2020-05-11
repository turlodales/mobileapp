using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Storage.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmTimeEntry : RealmObject, IDatabaseTimeEntry
    {
        public bool ContainsBackup { get; set; }

        public bool BillableBackup { get; set; }

        public DateTimeOffset StartBackup { get; set; }

        public long? DurationBackup { get; set; }

        public string DescriptionBackup { get; set; }

        public long? ProjectIdBackup { get; set; }

        public long? TaskIdBackup { get; set; }

        public IList<long> TagIdsBackup { get; }
    }
}
