using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Network;
using System.Net.Http.Headers;

namespace Toggl.Core.Tests.Helpers
{
    public static class ApiExceptions
    {
        private sealed class MockHttpHeaders : HttpHeaders
        {
        }

        internal static IRequest Request => new Request("mock body", new Uri("http://mock.request.com"), Enumerable.Empty<HttpHeader>(), HttpMethod.Get);

        internal static IResponse Response => new Response("mock body", false, "application/json", new MockHttpHeaders(), (HttpStatusCode)419);

        public static IEnumerable<object[]> ClientExceptions
            => new[]
            {
                new object[] { new BadRequestException(Request, Response) },
                new object[] { new UnauthorizedException(Request, Response) },
                new object[] { new PaymentRequiredException(Request, Response) },
                new object[] { new ForbiddenException(Request, Response) },
                new object[] { new NotFoundException(Request, Response) },
                new object[] { new ApiDeprecatedException(Request, Response) },
                new object[] { new RequestEntityTooLargeException(Request, Response) },
                new object[] { new ClientDeprecatedException(Request, Response) },
                new object[] { new TooManyRequestsException(Request, Response) }
            };

        public static IEnumerable<object[]> ClientExceptionsWhichAreNotReThrownInSyncStates
            => new[]
            {
                new object[] { new BadRequestException(Request, Response) },
                new object[] { new PaymentRequiredException(Request, Response) },
                new object[] { new ForbiddenException(Request, Response) },
                new object[] { new NotFoundException(Request, Response) },
                new object[] { new RequestEntityTooLargeException(Request, Response) },
                new object[] { new TooManyRequestsException(Request, Response) }
            };

        public static IEnumerable<object[]> ExceptionsWhichCauseRethrow()
            => new[]
            {
                new object[] { new ClientDeprecatedException(Request, Response) },
                new object[] { new ApiDeprecatedException(Request, Response) },
                new object[] { new UnauthorizedException(Request, Response) }
            };

        public static IEnumerable<object[]> ServerExceptions
            => new[]
            {
                new object[] { new InternalServerErrorException(Request, Response) },
                new object[] { new BadGatewayException(Request, Response) },
                new object[] { new GatewayTimeoutException(Request, Response) },
                new object[] { new HttpVersionNotSupportedException(Request, Response) },
                new object[] { new ServiceUnavailableException(Request, Response) }
            };
    }
}
