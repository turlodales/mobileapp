using System.Net;
using Toggl.Networking.Network;

namespace Toggl.Networking.Exceptions
{
    public sealed class InvalidConfirmationCodeException : ClientErrorException
    {
        private const string defaultMessage = "Invalid or expired confirmation code";

        internal InvalidConfirmationCodeException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal InvalidConfirmationCodeException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
