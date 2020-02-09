using System;
using Toggl.Shared;
using Math = System.Math;

namespace Toggl.Networking.Network
{
    /// <summary>
    /// This implementation of the leaky bucket tries to emulate the behavior of the nginx rate limiting
    /// algorith with the assumption that the `nodelay` option is turned on.
    ///
    /// Resources:
    /// - http://nginx.org/en/docs/http/ngx_http_limit_req_module.html#limit_req
    /// - https://www.freecodecamp.org/news/nginx-rate-limiting-in-a-nutshell-128fe9e0126c/
    /// </summary>
    public sealed class LeakyBucket
    {
        private readonly Func<DateTimeOffset> currentTime;

        private readonly object mutex = new object();

        private DateTimeOffset timeOfLastUpdate;
        private uint remainingSlots;
        private uint maximumNumberOfAvailableSlots;

        public TimeSpan SlotDuration { get; private set; }
        public uint Size { get; private set; }

        public LeakyBucket(Func<DateTimeOffset> currentTime, uint size = 60, uint burstSize = 0)
        {
            Ensure.Argument.IsNotNull(currentTime, nameof(currentTime));

            this.currentTime = currentTime;

            ChangeSize(size, burstSize);

            timeOfLastUpdate = currentTime();
            remainingSlots = maximumNumberOfAvailableSlots;
        }

        public void ChangeSize(uint size, uint burstSize)
        {
            if (size == 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size), @"The leaky bucket has to allow at least 1 request per minute.");

            lock (mutex)
            {
                var currentlyUsedSlots = maximumNumberOfAvailableSlots - remainingSlots;

                Size = size;
                SlotDuration = TimeSpan.FromMinutes(1) / size;

                maximumNumberOfAvailableSlots = burstSize + 1;
                remainingSlots = Math.Max(maximumNumberOfAvailableSlots - currentlyUsedSlots, 0);
            }
        }

        public bool TryClaimSlot()
        {
            lock (mutex)
            {
                freeSlotsIfPossibleAndNeeded();

                if (remainingSlots == 0)
                {
                    return false;
                }

                claimSlot();
                return true;
            }
        }

        private void freeSlotsIfPossibleAndNeeded()
        {
            var n = calculateNumberOfSlotsToFree();
            if (n > 0)
            {
                remainingSlots += n;
                timeOfLastUpdate = currentTime();
            }
        }

        private void claimSlot()
        {
            --remainingSlots;
            timeOfLastUpdate = currentTime();
        }

        private uint calculateNumberOfSlotsToFree()
        {
            var timeSinceLastUpdate = currentTime() - timeOfLastUpdate;
            var numberOfSlotsWhichExpired = (uint)Math.Floor(timeSinceLastUpdate / SlotDuration);
            var maximumNumberOfSlotsWhichCouldBeFreed = maximumNumberOfAvailableSlots - remainingSlots;
            return Math.Min(numberOfSlotsWhichExpired, maximumNumberOfSlotsWhichCouldBeFreed);
        }
    }
}
