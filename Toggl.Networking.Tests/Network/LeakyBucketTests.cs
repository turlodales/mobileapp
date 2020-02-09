using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using System;
using Toggl.Networking.Network;
using Xunit;

namespace Toggl.Networking.Tests
{
    public sealed class LeakyBucketTests
    {
        public abstract class LeakyBucketTestsBase
        {
            protected Func<DateTimeOffset> CurrentTime { get; } = Substitute.For<Func<DateTimeOffset>>();
        }

        public sealed class TheConstructor : LeakyBucketTestsBase
        {
            [Fact, LogIfTooSlow]
            public void ThrowsWhenAnArgumentIsNull()
            {
                Action creatingLeakyBucket = () => new LeakyBucket(null, 1);

                creatingLeakyBucket.Should().Throw<ArgumentNullException>();
            }

            [Fact, LogIfTooSlow]
            public void ThrowsWhenTheSizeIsSetToZero()
            {
                Action creatingLeakyBucket = () => new LeakyBucket(CurrentTime, 0);

                creatingLeakyBucket.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(1, 60.0)]
            [InlineData(2, 30.0)]
            [InlineData(60, 1.0)]
            [InlineData(80, 0.75)]
            [InlineData(200, 0.3)]
            public void CalculatesSlotDuration(uint size, double durationSeconds)
            {
                var bucket = new LeakyBucket(CurrentTime, size);

                bucket.SlotDuration.TotalSeconds.Should().BeApproximately(durationSeconds, 0.01);
            }
        }

        public sealed class TheChangeSizeMethod : LeakyBucketTestsBase
        {
            [Fact, LogIfTooSlow]
            public void ThrowsWhenTheSizeIsSetToZero()
            {
                var bucket = new LeakyBucket(CurrentTime, 1);

                Action creatingLeakyBucket = () => bucket.ChangeSize(0, 0);

                creatingLeakyBucket.Should().Throw<ArgumentOutOfRangeException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(1, 60.0)]
            [InlineData(2, 30.0)]
            [InlineData(60, 1.0)]
            [InlineData(80, 0.75)]
            [InlineData(200, 0.3)]
            public void CalculatesSlotDuration(uint size, double durationSeconds)
            {
                var bucket = new LeakyBucket(CurrentTime, 1);

                bucket.ChangeSize(size, 0);

                bucket.SlotDuration.TotalSeconds.Should().BeApproximately(durationSeconds, 0.01);
            }
        }

        public sealed class TheTryClaimSlotMethod : LeakyBucketTestsBase
        {
            private readonly DateTimeOffset baseTime
                = new DateTimeOffset(2018, 12, 1, 22, 12, 24, TimeSpan.FromHours(6));

            [Property]
            public void ClaimingFreeSlotAlwaysSucceeds(uint size)
            {
                if (size == 0) return;

                CurrentTime.Invoke().Returns(baseTime);
                var bucket = new LeakyBucket(CurrentTime, size);

                var claimed = bucket.TryClaimSlot();

                claimed.Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalseForASecondAttemptIfThereAreNoBursts()
            {
                CurrentTime.Invoke().Returns(baseTime);
                var bucket = new LeakyBucket(CurrentTime, 5, 0);

                bucket.TryClaimSlot();
                var claimed = bucket.TryClaimSlot();

                claimed.Should().BeFalse();
            }

            [Fact]
            public void ReturnsTrueForASecondAttemptIfThereAreNoBurstsButEnoughTimePassesBetweenRequests()
            {
                var size = 1u;
                var slotDuration = TimeSpan.FromSeconds(60.0 / size);
                CurrentTime.Invoke().Returns(baseTime);
                var bucket = new LeakyBucket(CurrentTime, size, 0);

                bucket.TryClaimSlot();
                CurrentTime.Invoke().Returns(baseTime + slotDuration + TimeSpan.FromMilliseconds(1));
                var claimed = bucket.TryClaimSlot();

                claimed.Should().BeTrue();
            }

            [Fact]
            public void ReturnsTrueForASecondAttemptIfBurstsAreEnabled()
            {
                var size = 1u;
                var slotDuration = TimeSpan.FromSeconds(60.0 / size);
                CurrentTime.Invoke().Returns(baseTime, baseTime, baseTime + slotDuration / 2);
                var bucket = new LeakyBucket(CurrentTime, size, 1);

                bucket.TryClaimSlot();
                var claimed = bucket.TryClaimSlot();

                claimed.Should().BeTrue();
            }

            [Property]
            public void HandlesBurstsCorrectly(uint burstSize)
            {
                CurrentTime.Invoke().Returns(baseTime);
                var bucket = new LeakyBucket(CurrentTime, 1, burstSize);

                bool claimed;

                // the first claim always succeeds
                claimed = bucket.TryClaimSlot();
                claimed.Should().BeTrue();

                for (int i = 0; i < burstSize; ++i)
                {
                    // the next `burstSize` claims succeed as well
                    claimed = bucket.TryClaimSlot();
                    claimed.Should().BeTrue();
                }

                // any extra claim fails
                claimed = bucket.TryClaimSlot();
                claimed.Should().BeFalse();
            }

            [Property]
            public void FreesOneSlotPerEveryTick(uint a, uint b)
            {
                if (a == 0 && b == 0) return;

                var (delay, burstSize) = (Math.Min(a, b), Math.Max(a, b));

                var bucket = new LeakyBucket(CurrentTime, 60, burstSize);

                // first we use up the whole "burst" of available slots
                CurrentTime.Invoke().Returns(baseTime);
                for (int i = 0; i < burstSize + 1; ++i)
                {
                    bucket.TryClaimSlot();
                }

                bool claimed;

                // after `delay` ticks, there should be exactly `delay` free slots
                CurrentTime.Invoke().Returns(baseTime + delay * bucket.SlotDuration);
                for (int i = 0; i < delay; ++i)
                {
                    claimed = bucket.TryClaimSlot();
                    claimed.Should().BeTrue();
                }

                // any extra request at the same time would be rejected
                claimed = bucket.TryClaimSlot();
                claimed.Should().BeFalse();
            }
        }
    }
}
