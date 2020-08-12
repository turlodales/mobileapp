using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Toggl.Networking.Exceptions
{
    public sealed class OfflineException : Exception
    {
        private const string defaultMessage = "Offline mode was detected.";

        public OfflineException()
            : base(defaultMessage)
        {
        }

        public OfflineException(Exception innerException)
            : base(defaultMessage, innerException)
        {
        }

        public bool HasTimeouted
            => InnerException is HttpRequestException httpEx && httpEx.InnerException is TaskCanceledException;

    }
}
