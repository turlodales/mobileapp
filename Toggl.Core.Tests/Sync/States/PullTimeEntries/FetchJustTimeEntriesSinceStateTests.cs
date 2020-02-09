using FluentAssertions;
using FsCheck.Xunit;
using Microsoft.Reactive.Testing;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Sync;
using Toggl.Core.Sync.States;
using Toggl.Core.Sync.States.PullTimeEntries;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking;
using Toggl.Shared.Models;
using Toggl.Storage;
using Xunit;

namespace Toggl.Core.Tests.Sync.States.PullTimeEntries
{
    public sealed class FetchJustTimeEntriesSinceStateTests
    {
        public sealed class TheStartMethod
        {
            private readonly ISinceParameterRepository sinceParameters;
            private readonly ITogglApi api;
            private readonly ITimeService timeService;
            private readonly FetchJustTimeEntriesSinceState state;
            private readonly DateTimeOffset now = new DateTimeOffset(2017, 02, 15, 13, 50, 00, TimeSpan.Zero);

            public TheStartMethod()
            {
                sinceParameters = Substitute.For<ISinceParameterRepository>();
                api = Substitute.For<ITogglApi>();
                timeService = Substitute.For<ITimeService>();
                timeService.CurrentDateTime.Returns(now);
                state = new FetchJustTimeEntriesSinceState(api, sinceParameters, timeService);
            }

            [Fact, LogIfTooSlow]
            public void EmitsTransitionToFetchStartedResult()
            {
                var transition = state.Start().SingleAsync().Wait();

                transition.Result.Should().Be(state.Done);
            }

            [Fact, LogIfTooSlow]
            public void MakesNoApiCallsBeforeSubscription()
            {
                state.Start();

                var t = api.DidNotReceive().TimeEntries;
            }

            [Property]
            public void MakesCorrectCallWithSinceThresholdsWhenSinceIsLessThanTwoMonthsInThePast(int seed)
            {
                var rnd = new Random(seed);
                var percent = rnd.NextDouble();

                var now = timeService.CurrentDateTime;
                var twoMonths = (now.AddMonths(2) - now);
                var seconds = twoMonths.TotalSeconds * percent;
                var since = now.AddSeconds(-seconds);

                sinceParameters.Get<ITimeEntry>().Returns(since);

                state.Start().Wait();

                api.TimeEntries.Received().GetAllSince(since);
            }

            [Fact, LogIfTooSlow]
            public void MakesApiCallsWithoutTheSinceParameterWhenTheThresholdIsMoreThanTwoMonthsInThePast()
            {
                var now = timeService.CurrentDateTime;

                sinceParameters.Get<ITimeEntry>().Returns(now.AddMonths(-8));

                state.Start().Wait();

                api.TimeEntries.Received().GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public void MakesCorrectCallWithoutSinceThreshold()
            {
                state.Start().Wait();

                api.TimeEntries.Received().GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public void ReturnsReplayingApiCallObservables()
            {
                api.TimeEntries
                    .GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .ReturnsTaskOf(null);

                var transition = (Transition<IFetchObservables>)state.Start().SingleAsync().Wait();

                var observables = transition.Parameter;
                observables.GetList<ITimeEntry>().SingleAsync().Wait().Should().BeNull();
            }

            [Fact, LogIfTooSlow]
            public async Task FetchesTwoMonthsOfTimeEntriesDataIncludingTwoDaysAfterNow()
            {
                sinceParameters.Get<ITimeEntry>().Returns(now.AddMonths(-8));

                await state.Start().SingleAsync();

                var max = TimeSpan.FromDays(62);
                var min = TimeSpan.FromDays(59);

                await api.TimeEntries.Received().GetAll(
                    Arg.Is<DateTimeOffset>(start => min <= now - start && now - start <= max), Arg.Is(now.AddDays(2)));
            }

            [Fact]
            public void AllTheOtherObservablesThrow()
            {
                var fetchObservables = ((Transition<IFetchObservables>)state.Start().SingleAsync().Wait()).Parameter;

                void throwsForList<T>()
                {
                    Action subscribe = () => fetchObservables.GetList<T>().Subscribe();

                    subscribe.Should().Throw<InvalidOperationException>();
                }

                void throwsForSingle<T>()
                {
                    Action subscribe = () => fetchObservables.GetSingle<T>().Subscribe();

                    subscribe.Should().Throw<InvalidOperationException>();
                }

                throwsForList<IWorkspace>();
                throwsForList<IProject>();
                throwsForList<ITask>();
                throwsForList<IClient>();
                throwsForList<ITag>();
                throwsForList<IWorkspaceFeatureCollection>();
                throwsForSingle<IUser>();
                throwsForSingle<IPreferences>();
            }
        }
    }
}
