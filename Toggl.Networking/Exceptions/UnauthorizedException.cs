using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Toggl.Networking.Network;

namespace Toggl.Networking.Exceptions
{
    public sealed class UnauthorizedException : ClientErrorException
    {
        public const HttpStatusCode CorrespondingHttpCode = HttpStatusCode.Unauthorized;

        private const string defaultMessage = "User is not authorized to make this request and must enter login again.";

        private readonly IEnumerable<HttpHeader> requestHeaders;
        private readonly string remainingLoginAttemptsHeaderName = "X-Remaining-Login-Attempts";

        public string ApiToken
            => requestHeaders
                .Where(header => header.Type == HttpHeader.HeaderType.Auth)
                .Select(tryDecodeApiToken)
                .FirstOrDefault(token => token != null);

        public int? RemainingLoginAttempts { get; }

        internal UnauthorizedException(IRequest request, IResponse response)
            : this(request, response, defaultMessage)
        {
        }

        internal UnauthorizedException(IRequest request, IResponse response, string errorMessage)
            : base(request, response, errorMessage)
        {
            requestHeaders = request.Headers;
            RemainingLoginAttempts = getRemainingLoginAttemptsFromHeaders(response.Headers);
        }

        private int? getRemainingLoginAttemptsFromHeaders(HttpHeaders headers)
        {
            if (headers == null)
            {
                return null;
            }

            var hasHeader = headers.TryGetValues(remainingLoginAttemptsHeaderName, out var remainingAttemptsHeaderValues);

            if (hasHeader && Int32.TryParse(remainingAttemptsHeaderValues.First(), out var remainingAttempts))
            {
                return remainingAttempts;
            }

            return null;
        }

        private static string tryDecodeApiToken(HttpHeader authHeader)
        {
            byte[] decodedBytes;
            try
            {
                decodedBytes = Convert.FromBase64String(authHeader.Value);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }

            var authString = Encoding.UTF8.GetString(decodedBytes, 0, decodedBytes.Length);

            var parts = authString.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || parts[1] != "api_token")
                return null;

            return parts[0];
        }
    }
}
