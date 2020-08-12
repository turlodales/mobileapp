using System;
using Toggl.Shared.Models;

namespace Toggl.Storage.Models
{
    public interface IDatabasePushRequestIdentifier : IIdentifiable
    {
        string PushRequestId { get; }
    }
}
