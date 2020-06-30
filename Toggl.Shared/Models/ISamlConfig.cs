using System;

namespace Toggl.Shared.Models
{
    public interface ISamlConfig
    {
        Uri SsoUrl { get; }
    }
}
