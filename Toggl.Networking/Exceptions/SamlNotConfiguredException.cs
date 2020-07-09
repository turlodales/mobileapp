using System.Net;
using Toggl.Networking.Network;

namespace Toggl.Networking.Exceptions
{
    public sealed class SamlNotConfiguredException : ClientErrorException
    {
        private const string defaultMessage = "SAML2 is not configured for the given email";

        internal SamlNotConfiguredException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal SamlNotConfiguredException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
        }
    }
}
