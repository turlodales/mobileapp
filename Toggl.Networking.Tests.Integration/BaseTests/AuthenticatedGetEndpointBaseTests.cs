using FluentAssertions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Networking.Network;
using Xunit;

namespace Toggl.Networking.Tests.Integration.BaseTests
{
    public abstract class AuthenticatedGetEndpointBaseTests<T> : AuthenticatedEndpointBaseTests<T>
    {
        [Fact, LogTestInfo]
        public async Task ReturnsTheSameWhetherUsingPasswordOrApiToken()
        {
            var (passwordClient, user) = await SetupTestUser();
            var apiTokenClient = TogglApiWith(Credentials.WithApiToken(user.ApiToken));

            var passwordReturn = await CallEndpointWith(passwordClient);
            var apiTokenReturn = await CallEndpointWith(apiTokenClient);

            passwordReturn.Should().BeEquivalentTo(apiTokenReturn);
        }

        [Fact, LogTestInfo]
        public async Task FetchesTheExactSameEntityWhenFetchingTwice()
        {
            var (validApi, user) = await SetupTestUser();

            var firstEntity = await CallEndpointWith(validApi);
            await Task.Delay(500).ConfigureAwait(false);
            var secondEntity = await CallEndpointWith(validApi);

            firstEntity.Should().BeEquivalentTo(secondEntity);
        }
    }
}
