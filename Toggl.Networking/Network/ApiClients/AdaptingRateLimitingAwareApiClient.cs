using System;
using System.Net;
using System.Threading.Tasks;
using Toggl.Shared;

namespace Toggl.Networking.Network
{
    internal sealed class AdaptingRateLimitingAwareApiClient : IApiClient
    {
        private const uint normalBucketSize = 80u;
        private const uint assumedBurstSize = 3u;
        private const uint noBursts = 0u;

        private readonly LeakyBucket leakyBucket;
        private readonly IApiClient internalClient;

        private bool burstsEnabled;

        public AdaptingRateLimitingAwareApiClient(IApiClient internalClient, Func<DateTimeOffset> currentTime)
        {
            Ensure.Argument.IsNotNull(internalClient, nameof(internalClient));
            Ensure.Argument.IsNotNull(currentTime, nameof(currentTime));

            leakyBucket = new LeakyBucket(currentTime, normalBucketSize, noBursts);
            this.internalClient = new RateLimitingAwareApiClient(internalClient, leakyBucket);

            burstsEnabled = true;
        }

        public void Dispose()
        {
            internalClient.Dispose();
        }

        public async Task<IResponse> Send(IRequest request)
        {
            var response = await internalClient.Send(request);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                burstsEnabled = false;
            }

            if (response.TryGetBucketSizeFromHeaders(out var size))
            {
                // We assume that under normal circumstances (there is no unexpected heavy load on the server) the burst
                // size is at the normal size (3). When the bucket size is decreased dynamically and is below a threshold (80),
                // we will assume that the burst size has dropped to zero as well. This isn't guaranteed by the ops team
                // though and it might be possible that the burst size is lower even when the bucket size drops.
                //
                // If we get a "Too Many Requests" error, we will stop sending requests in bursts until the app restarts
                // or until the size of the bucket increases.

                lock (leakyBucket)
                {
                    if (burstsEnabled && size > leakyBucket.Size && response.StatusCode != HttpStatusCode.TooManyRequests)
                    {
                        burstsEnabled = true;
                    }

                    var burstSize = size >= normalBucketSize && !burstsEnabled ? assumedBurstSize : noBursts;
                    leakyBucket.ChangeSize(size, burstSize);
                }
            }

            return response;
        }
    }
}
