using System;
using System.Linq;
using Toggl.Shared;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal sealed class PushRequestIdentifierStorage : IPushRequestIdentifierRepository
    {
        private const long onlyId = 1;

        private readonly object storageAccess = new object();
        private readonly IRealmAdapter<IDatabasePushRequestIdentifier> realmAdapter;

        public PushRequestIdentifierStorage(IRealmAdapter<IDatabasePushRequestIdentifier> realmAdapter)
        {
            Ensure.Argument.IsNotNull(realmAdapter, nameof(realmAdapter));

            this.realmAdapter = realmAdapter;
        }

        public void Clear()
        {
            lock (storageAccess)
            {
                try
                {
                    var record = realmAdapter.GetAll().FirstOrDefault();
                    if (record == null)
                        return;

                    realmAdapter.Delete(onlyId);
                }
                catch
                {
                }
            }
        }

        public bool TryGet(out Guid identifier)
        {
            lock (storageAccess)
            {
                try
                {
                    var pushRequestDb = realmAdapter.Get(onlyId);

                    if (pushRequestDb != null)
                    {
                        identifier = Guid.Parse(pushRequestDb.PushRequestId);
                        return true;
                    }
                }
                catch
                {
                }

                identifier = Guid.Empty;
                return false;
            }
        }

        public void Set(Guid identifier)
        {
            var record = new RealmPushRequestIdentifier
            {
                Id = onlyId,
                PushRequestId = identifier.ToString()
            };

            lock (storageAccess)
            {
                try
                {
                    realmAdapter.Update(onlyId, record);
                }
                catch (InvalidOperationException)
                {
                    realmAdapter.Create(record);
                }
            }
        }
    }
}
