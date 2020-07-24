using System;

namespace Toggl.Storage
{
public interface IPushRequestIdentifierRepository
{
    bool TryGet(out Guid identifier);
    void Set(Guid identifier);
    void Clear();
}
}
