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
using Toggl.Core.Sync.States.Pull;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking;
using Toggl.Shared.Models;
using Toggl.Storage;
using Xunit;

namespace Toggl.Core.Tests.Sync.States.Pull
{
    public sealed class FetchAllSinceStateTests
    {
        public sealed class TheStartMethod
        {
            private readonly ISinceParameterRepository sinceParameters;
            private readonly ITogglApi api;
            private readonly ITimeService timeService;
            private readonly FetchAllSinceState state;
            private readonly DateTimeOffset now = new DateTimeOffset(2017, 02, 15, 13, 50, 00, TimeSpan.Zero);

            public TheStartMethod()
            {
                sinceParameters = Substitute.For<ISinceParameterRepository>();
                api = Substitute.For<ITogglApi>();
                timeService = Substitute.For<ITimeService>();
                timeService.CurrentDateTime.Returns(now);
                state = new FetchAllSinceState(api, sinceParameters, timeService);
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

                var w = api.DidNotReceive().Workspaces;
                var wf = api.DidNotReceive().WorkspaceFeatures;
                var u = api.DidNotReceive().User;
                var c = api.DidNotReceive().Clients;
                var p = api.DidNotReceive().Projects;
                var t = api.DidNotReceive().TimeEntries;
                var ta = api.DidNotReceive().Tags;
                var ts = api.DidNotReceive().Tasks;
                var pr = api.DidNotReceive().Preferences;
            }

            [Property]
            public void MakesCorrectCallsWithSinceThresholdsWhenSinceIsLessThanTwoMonthsInThePast(int seed)
            {
                var rnd = new Random(seed);
                var percent = rnd.NextDouble();

                var now = timeService.CurrentDateTime;
                var twoMonths = (now.AddMonths(2) - now);
                var seconds = twoMonths.TotalSeconds * percent;
                var since = now.AddSeconds(-seconds);

                sinceParameters.Get<IClient>().Returns(since);
                sinceParameters.Get<IProject>().Returns(since);
                sinceParameters.Get<ITask>().Returns(since);
                sinceParameters.Get<ITag>().Returns(since);
                sinceParameters.Get<IWorkspace>().Returns(since);
                sinceParameters.Get<ITimeEntry>().Returns(since);

                state.Start().Wait();

                api.Workspaces.Received().GetAll();
                api.Clients.Received().GetAllSince(since);
                api.Projects.Received().GetAllSince(since);
                api.TimeEntries.Received().GetAllSince(since);
                api.Tasks.Received().GetAllSince(since);
                api.Tags.Received().GetAllSince(since);
            }

            [Fact, LogIfTooSlow]
            public void MakesApiCallsWithoutTheSinceParameterWhenTheThresholdIsMoreThanTwoMonthsInThePast()
            {
                var now = timeService.CurrentDateTime;

                sinceParameters.Get<IClient>().Returns(now.AddMonths(-3));
                sinceParameters.Get<IProject>().Returns(now.AddMonths(-4));
                sinceParameters.Get<ITask>().Returns(now.AddMonths(-5));
                sinceParameters.Get<ITag>().Returns(now.AddMonths(-6));
                sinceParameters.Get<IWorkspace>().Returns(now.AddMonths(-7));
                sinceParameters.Get<ITimeEntry>().Returns(now.AddMonths(-8));

                state.Start().Wait();

                api.Workspaces.Received().GetAll();
                api.Clients.Received().GetAll();
                api.Projects.Received().GetAll();
                api.TimeEntries.Received().GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());
                api.Tasks.Received().GetAll();
                api.Tags.Received().GetAll();
            }

            [Fact, LogIfTooSlow]
            public void MakesCorrectCallsWithoutSinceThresholds()
            {
                state.Start().Wait();

                api.Workspaces.Received().GetAll();
                api.Clients.Received().GetAll();
                api.Projects.Received().GetAll();
                api.TimeEntries.Received().GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());
                api.Tasks.Received().GetAll();
                api.Tags.Received().GetAll();
            }

            [Fact, LogIfTooSlow]
            public void ReturnsReplayingApiCallObservables()
            {
                api.Workspaces.GetAll().ReturnsTaskOf(null);
                api.WorkspaceFeatures.GetAll().ReturnsTaskOf(null);
                api.User.Get().ReturnsTaskOf(null);
                api.Clients.GetAll().ReturnsTaskOf(null);
                api.Projects.GetAll().ReturnsTaskOf(null);
                api.TimeEntries.GetAll(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>()).ReturnsTaskOf(null);
                api.Tasks.GetAll().ReturnsTaskOf(null);
                api.Tags.GetAll().ReturnsTaskOf(null);
                api.Preferences.Get().ReturnsTaskOf(null);

                var transition = (Transition<IFetchObservables>)state.Start().SingleAsync().Wait();

                var observables = transition.Parameter;
                observables.GetList<IWorkspace>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<IWorkspaceFeatureCollection>().SingleAsync().Wait().Should().BeNull();
                observables.GetSingle<IUser>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<IClient>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<IProject>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<ITimeEntry>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<ITask>().SingleAsync().Wait().Should().BeNull();
                observables.GetList<ITag>().SingleAsync().Wait().Should().BeNull();
                observables.GetSingle<IPreferences>().SingleAsync().Wait().Should().BeNull();
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
        }
    }
}
