﻿using System;
using Newtonsoft.Json;
using Toggl.Shared.Models;

namespace Toggl.Networking.Models
{
    internal sealed partial class SamlConfig : ISamlConfig
    {
        [JsonProperty("sso_url")]
        [JsonRequired]
        public Uri SsoUrl { get; set; }
    }
}
