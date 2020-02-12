using System;
using Toggl.Core.Helper;

namespace Toggl.Core.Exceptions
{
    public sealed class ThirdPartyLoginException : Exception
    {
        public ThirdPartyLoginProvider Provider { get; }
        public bool LoginWasCanceled { get; }

        public ThirdPartyLoginException(ThirdPartyLoginProvider provider, bool loginWasCanceled)
        {
            Provider = provider;
            LoginWasCanceled = loginWasCanceled;
        }
    }
}
