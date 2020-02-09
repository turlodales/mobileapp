using System.Threading.Tasks;
using Toggl.Networking.Network;

namespace Toggl.Networking.Extensions
{
    public static class LeakyBucketExtensions
    {
        public static async Task WaitForSlot(this LeakyBucket leakyBucket)
        {
            while (!leakyBucket.TryClaimSlot())
            {
                await Task.Delay(leakyBucket.SlotDuration);
            }
        }
    }
}
