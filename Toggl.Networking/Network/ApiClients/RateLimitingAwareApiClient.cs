using System;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Networking.Extensions;

namespace Toggl.Networking.Network
{
    internal sealed class RateLimitingAwareApiClient : IApiClient
    {
        private readonly LeakyBucket leakyBucket;
        private readonly IApiClient internalClient;

        public RateLimitingAwareApiClient(IApiClient internalClient, LeakyBucket leakyBucket)
        {
            this.internalClient = internalClient;
            this.leakyBucket = leakyBucket;
        }

        public void Dispose()
        {
            internalClient.Dispose();
        }

        public async Task<IResponse> Send(IRequest request)
        {
            await leakyBucket.WaitForSlot();

            return await internalClient.Send(request);
        }
    }
}
