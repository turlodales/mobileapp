using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.DataSources.Interfaces;
using Toggl.Core.Interactors;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Storage.Models;
using Toggl.Networking.Sync.Push;
using Xunit;
using System.Linq;
using static Toggl.Storage.SyncStatus;
using Toggl.Networking.Network;

namespace Toggl.Core.Tests.Interactors.Workspace
{
    public sealed class PreparePushRequestInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource)
            {
                Action tryingToConstructWithNull = () =>
                    new PreparePushRequestInteractor(UserAgent.ToString(), useDataSource ? DataSource : null);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly PreparePushRequestInteractor interactor;

            public TheExecuteMethod()
            {
                interactor = new PreparePushRequestInteractor(UserAgent.ToString(), DataSource);

                returnsValueOrEmpty(DataSource.Projects);
                returnsValueOrEmpty(DataSource.Tasks);
                returnsValueOrEmpty(DataSource.Clients);
                returnsValueOrEmpty(DataSource.Tags);
                returnsValueOrEmpty(DataSource.Workspaces);
                returnsValueOrEmpty(DataSource.TimeEntries);

                returnsValue(DataSource.User, new MockUser());
                returnsValue(DataSource.Preferences, new MockPreferences());
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoEntities()
            {
                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyTags()
            {
                returnsValueOrEmpty(DataSource.Tags, new[] { new MockTag(), new MockTag() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyClients()
            {
                returnsValueOrEmpty(DataSource.Clients, new[] { new MockClient(), new MockClient() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyTasks()
            {
                returnsValueOrEmpty(DataSource.Tasks, new[] { new MockTask(), new MockTask() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyProjects()
            {
                returnsValueOrEmpty(DataSource.Projects, new[] { new MockProject(), new MockProject() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyTimeEntries()
            {
                returnsValueOrEmpty(DataSource.TimeEntries, new[] { new MockTimeEntry(), new MockTimeEntry() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsClientsCreations()
            {
                var clients = new[] {
                    new MockClient() { Id = -1, SyncStatus = SyncNeeded },
                    new MockClient() { Id = -2, SyncStatus = SyncNeeded  }
                };
                var createdClientsCount = clients.Count(t => t.Id < 0);
                returnsValueOrEmpty(DataSource.Clients, clients);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.Clients.Count.Should().Be(createdClientsCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsProjectsCreations()
            {
                var projects = new[] {
                    new MockProject() { Id = -1, SyncStatus = SyncNeeded , Color = "#000000" },
                    new MockProject() { Id = -2, SyncStatus = SyncNeeded , Color = "#000000" }
                };
                var createdProjectsCount = projects.Count(t => t.Id < 0);
                returnsValueOrEmpty(DataSource.Projects, projects);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.Projects.Count.Should().Be(createdProjectsCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTagsCreations()
            {
                var tags = new[] {
                    new MockTag() { Id = -1, SyncStatus = SyncNeeded  },
                    new MockTag() { Id = -2, SyncStatus = SyncNeeded  }
                };
                var createdTagsCount = tags.Count(t => t.Id < 0);
                returnsValueOrEmpty(DataSource.Tags, tags);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.Tags.Count.Should().Be(createdTagsCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsPreferencesChanges()
            {
                var preferences = new MockPreferences() { SyncStatus = SyncNeeded };
                returnsValue(DataSource.Preferences, preferences);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.Preferences.Should().NotBeNull();
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsUserChanges()
            {
                var user = new MockUser() { SyncStatus = SyncNeeded };
                returnsValue(DataSource.User, user);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.User.Should().NotBeNull();
            }

            private MockTimeEntry[] createTimeEntries()
                => new[] {
                    new MockTimeEntry() { Id = -100, SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = -200, SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = -300, SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = 1, Description = "ABC", SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = 2, Description = "DEF", SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = 99, SyncStatus = SyncNeeded, IsDeleted = true },
                    new MockTimeEntry() { Id = -99, SyncStatus = SyncNeeded, IsDeleted = true }
                };

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryCreations()
            {
                var timeEntries = createTimeEntries();
                var createdTimeEntriesCount = timeEntries.Count(t => t.Id < 0 && !t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.Where(a => a.Type == ActionType.Create).Should().HaveCount(createdTimeEntriesCount);
            }

            [Fact, LogIfTooSlow]
            public async Task TimeEntryCreationsHaveACorrectUserAgent()
            {
                var userAgentString = UserAgent.ToString();
                returnsValueOrEmpty(DataSource.TimeEntries, createTimeEntries());

                var request = await interactor.Execute();

                request.TimeEntries
                    .Where(a => a.Type == ActionType.Create)
                    .All(te => ((Networking.Models.TimeEntry)te.Payload).CreatedWith == userAgentString)
                    .Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task RequestDoesNotContainTimeEntryCreationsForLocalDeletes()
            {
                var timeEntries = createTimeEntries();
                var createdTimeEntriesCount = timeEntries.Count(t => t.Id < 0 && !t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.Where(a => a.Type == ActionType.Create).Should().HaveCount(createdTimeEntriesCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryUpdates()
            {
                var timeEntries = createTimeEntries();
                var updatedTimeEntriesCount = timeEntries.Count(t => t.Id >= 0 && t.SyncStatus == SyncNeeded && !t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.Where(a => a.Type == ActionType.Update).Should().HaveCount(updatedTimeEntriesCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryDeletions()
            {
                var timeEntries = createTimeEntries();
                var deletedTimeEntriesCount = timeEntries.Count(t => t.Id >= 0 && t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.Where(a => a.Type == ActionType.Delete).Should().HaveCount(deletedTimeEntriesCount);
            }

            private void returnsValueOrEmpty<TThreadsafe, TDatabase>(IDataSource<TThreadsafe, TDatabase> dataSource, IEnumerable<TThreadsafe> returnValue = null)
                where TDatabase : IDatabaseModel
                where TThreadsafe : TDatabase, IThreadSafeModel
            {
                returnValue ??= Array.Empty<TThreadsafe>();

                dataSource
                    .GetAll(Arg.Any<Func<TDatabase, bool>>(), Arg.Any<bool>())
                    .Returns(callInfo => Observable.Return(returnValue.Where(item => callInfo.ArgAt<Func<TDatabase, bool>>(0)(item))));
            }

            private void returnsValue<TThreadsafe>(ISingletonDataSource<TThreadsafe> dataSource, TThreadsafe value)
              where TThreadsafe : IThreadSafeModel
            {
                dataSource.Get().Returns(Observable.Return(value));
            }
        }
    }
}
