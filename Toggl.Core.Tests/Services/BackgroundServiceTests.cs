using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Services;
using Toggl.Core.Tests.Generators;
using Xunit;

namespace Toggl.Core.Tests.Services
{
    public sealed class BackgroundServiceTests
    {
        public abstract class BackgroundServiceTest
        {
            protected ITimeService TimeService { get; }
            protected IAnalyticsService AnalyticsService { get; }
            protected IUpdateRemoteConfigCacheService UpdateRemoteConfigCacheService { get; }
            protected IInteractorFactory InteractorFactory { get; }

            public BackgroundServiceTest()
            {
                TimeService = Substitute.For<ITimeService>();
                AnalyticsService = Substitute.For<IAnalyticsService>();
                UpdateRemoteConfigCacheService = Substitute.For<IUpdateRemoteConfigCacheService>();
                InteractorFactory = Substitute.For<IInteractorFactory>();
            }
        }

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsWhenTheArgumentIsNull(bool useTimeService, bool useAnalyticsService, bool useRemoteConfigUpdateService, bool useInteractorFactory)
            {
                var timeService = useTimeService ? Substitute.For<ITimeService>() : null;
                var analyticsService = useAnalyticsService ? Substitute.For<IAnalyticsService>() : null;
                var updateRemoteConfigCacheService = useRemoteConfigUpdateService ? Substitute.For<IUpdateRemoteConfigCacheService>() : null;
                var interactorFactory = useInteractorFactory ? Substitute.For<IInteractorFactory>() : null;
                Action constructor = () => new BackgroundService(timeService, analyticsService, updateRemoteConfigCacheService, interactorFactory);

                constructor.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheAppResumedFromBackgroundMethod : BackgroundServiceTest
        {
            private readonly DateTimeOffset now = new DateTimeOffset(2017, 12, 11, 0, 30, 59, TimeSpan.Zero);

            [Fact, LogIfTooSlow]
            public void DoesNotEmitAnythingWhenItHasNotEnterBackgroundFirst()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterForeground();

                emitted.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void EmitsValueWhenEnteringForegroundAfterBeingInBackground()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                TimeService.CurrentDateTime.Returns(now);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterBackground();
                backgroundService.EnterForeground();

                emitted.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotEmitAnythingWhenTheEnterForegroundIsCalledMultipleTimes()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                TimeService.CurrentDateTime.Returns(now);
                backgroundService.EnterBackground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(1));
                backgroundService.EnterForeground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(2));
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterForeground();

                emitted.Should().BeFalse();
            }

            [Property]
            public void EmitsAValueWhenEnteringForegroundAfterBeingInBackgroundForMoreThanTheLimit(NonNegativeInt waitingTime)
            {
                TimeSpan? resumedAfter = null;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(timeInBackground => resumedAfter = timeInBackground);
                TimeService.CurrentDateTime.Returns(now);

                backgroundService.EnterBackground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(waitingTime.Get).AddSeconds(1));
                backgroundService.EnterForeground();

                resumedAfter.Should().NotBeNull();
                resumedAfter.Should().BeGreaterThan(TimeSpan.FromMinutes(waitingTime.Get));
            }

            [Fact]
            public void TracksEventWhenAppResumed()
            {
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                backgroundService.EnterBackground();
                backgroundService.EnterForeground();
                AnalyticsService.Received().AppDidEnterForeground.Track();
            }

            [Fact]
            public void TracksEventWhenAppGoesToBackground()
            {
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, InteractorFactory);
                backgroundService.EnterBackground();
                AnalyticsService.Received().AppSentToBackground.Track();
            }
        }

        public sealed class TheEnterForegroundMethod : BackgroundServiceTest
        {
            [Fact, LogIfTooSlow]
            public async Task TriggersRemoteConfigUpdateWhenRemoteConfigDataNeedsToBeUpdated()
            {
                var updateRemoteConfigCacheService = Substitute.For<IUpdateRemoteConfigCacheService>();
                updateRemoteConfigCacheService.NeedsToUpdateStoredRemoteConfigData().Returns(true);
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, updateRemoteConfigCacheService, InteractorFactory);

                backgroundService.EnterForeground();

                // This delay is make sure FetchAndStoreRemoteConfigData has time to execute, since it's called inside a
                // fire and forget TaskTask.Run(() => {}).ConfigureAwait(false))
                await Task.Delay(1);
                updateRemoteConfigCacheService.Received().FetchAndStoreRemoteConfigData();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotTriggerRemoteConfigUpdateWhenRemoteConfigDataDoesNotNeedToBeUpdated()
            {
                var updateRemoteConfigCacheService = Substitute.For<IUpdateRemoteConfigCacheService>();
                updateRemoteConfigCacheService.NeedsToUpdateStoredRemoteConfigData().Returns(false);
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, updateRemoteConfigCacheService, InteractorFactory);

                backgroundService.EnterForeground();

                // This delay is make sure FetchAndStoreRemoteConfigData has time to execute, since it's called inside a
                // fire and forget TaskTask.Run(() => {}).ConfigureAwait(false))
                await Task.Delay(1);
                updateRemoteConfigCacheService.DidNotReceive().FetchAndStoreRemoteConfigData();
            }
            
            [Fact, LogIfTooSlow]
            public async Task TriggersEventNotificationsUpdate()
            {
                var updateRemoteConfigCacheService = Substitute.For<IUpdateRemoteConfigCacheService>();
                updateRemoteConfigCacheService.NeedsToUpdateStoredRemoteConfigData().Returns(false);
                var backgroundService = new BackgroundService(TimeService, AnalyticsService, updateRemoteConfigCacheService, InteractorFactory);

                backgroundService.EnterForeground();
                
                // This delay is make sure UpdateEventNotificationsSchedules has time to execute, since it's called inside a
                // fire and forget TaskTask.Run(() => {}).ConfigureAwait(false))
                await Task.Delay(1);
                await InteractorFactory.Received().UpdateEventNotificationsSchedules().Execute();
            }
        }
    }
}
