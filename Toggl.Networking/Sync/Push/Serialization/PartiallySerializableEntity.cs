using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Toggl.Networking.Models;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;

namespace Toggl.Networking.Sync.Push.Serialization
{
    internal static class PartiallySerializableEntityFactory
    {
        public static IPartiallySerializableEntity Create<TEntity>(TEntity entity, TEntity backedUpEntity)
        {
            if (typeof(TEntity).ImplementsOrDerivesFrom(typeof(ITimeEntry)))
            {
                return new PartiallySerializableTimeEntry(entity as ITimeEntry, backedUpEntity as ITimeEntry);
            }

            throw new NotImplementedException();
        }
    }
}
