using System.Net;
using Toggl.Networking.Network;

namespace Toggl.Networking.Exceptions
{
    public sealed class BadSsoEmailException : ClientErrorException
    {
        private const string defaultMessage = "User email doesn't match SSO credentials";

        internal BadSsoEmailException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal BadSsoEmailException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
