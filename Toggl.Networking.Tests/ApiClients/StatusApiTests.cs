using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Networking.ApiClients;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Network;
using Toggl.Networking.Tests.Exceptions;
using Toggl.Networking.Tests.Network;
using Xunit;
using static System.Net.HttpStatusCode;

namespace Toggl.Networking.Tests.Clients
{
    public sealed class StatusClientTests
    {
        public sealed class TheIsAvailableMethod
        {
            private readonly StatusApi statusApi;
            private readonly IApiClient apiClient = Substitute.For<IApiClient>();

            public TheIsAvailableMethod()
            {
                var endpoints = new Endpoints(ApiEnvironment.Staging);
                statusApi = new StatusApi(endpoints, apiClient);
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotHideThrownExceptions()
            {
                bool caughtException = false;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                apiClient
                    .Send(Arg.Any<IRequest>())
                    .Returns(async x => throw new WebException());
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

                try
                {
                    await statusApi.IsAvailable();
                }
                catch
                {
                    caughtException = true;
                }

                caughtException.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void ReturnsASingleValueForWorkingApiCalls()
            {
                apiClient
                    .Send(Arg.Any<IRequest>())
                    .Returns(x => new Response("OK", true, "text/plain", new MockHttpHeaders(), OK));

                statusApi.IsAvailable().Wait();
            }

            [Fact, LogIfTooSlow]
            public void ThrowsApiExceptionExceptionWhenApiServerIsNotAvailable()
            {
                apiClient
                    .Send(Arg.Any<IRequest>())
                    .Returns(x => new Response("PANIC", false, "text/plain", new MockHttpHeaders(), InternalServerError));

                Action gettingServerStatus = () => statusApi.IsAvailable().Wait();

                gettingServerStatus.Should().Throw<InternalServerErrorException>();
            }
        }
    }
}
