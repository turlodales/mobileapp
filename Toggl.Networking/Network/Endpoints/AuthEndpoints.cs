using System;

namespace Toggl.Networking.Network
{
    internal struct AuthEndpoints
    {
        private readonly Uri baseUrl;

        public AuthEndpoints(Uri baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public Endpoint Get => Endpoint.Get(baseUrl, "auth/saml2/login");
    }
}
