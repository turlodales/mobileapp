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
using Xunit;
using System.Linq;
using Toggl.Networking.Sync.Push;
using static Toggl.Storage.SyncStatus;
using Toggl.Networking.Extensions;

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
                    new PreparePushRequestInteractor(useDataSource ? DataSource : null);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly PreparePushRequestInteractor interactor;

            public TheExecuteMethod()
            {
                interactor = new PreparePushRequestInteractor(DataSource);

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
            public async Task EmitsEmptyRequestWhenThereAreNoDirtyEntities()
            {
                returnsValueOrEmpty(DataSource.Tags, new[] { new MockTag(), new MockTag() });

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsClientsCreations()
            {
                var clients = new[] {
                    new MockClient() { Id = -1 },
                    new MockClient() { Id = -2 }
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
                    new MockProject() { Id = -1 }.WithColor("#000000"),
                    new MockProject() { Id = -2 }.WithColor("#000000")
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
                    new MockTag() { Id = -1 },
                    new MockTag() { Id = -2 }
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
                    new MockTimeEntry() { Id = 99, SyncStatus = SyncNeeded, IsDeleted = true }
                };

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryCreations()
            {
                var timeEntries = createTimeEntries();
                var createdTimeEntriesCount = timeEntries.Count(t => t.Id < 0);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.WhereMatchesGenericType(typeof(CreateAction<>)).Should().HaveCount(createdTimeEntriesCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryUpdates()
            {
                var backupUpdates = new IThreadSafeTimeEntry[]
                {
                    new MockTimeEntry() { Id = 1, Description = "ABC-changed", SyncStatus = SyncNeeded },
                    new MockTimeEntry() { Id = 2, Description = "DEF-changed", SyncStatus = SyncNeeded },
                };
                DataSource.TimeEntries.GetBackedUpTimeEntries().Returns(Observable.Return(backupUpdates));

                var timeEntries = createTimeEntries();
                var updatedTimeEntriesCount = timeEntries.Count(t => t.Id >= 0 && t.SyncStatus == SyncNeeded && !t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.WhereMatchesGenericType(typeof(UpdateAction<>)).Should().HaveCount(updatedTimeEntriesCount);
            }

            [Fact, LogIfTooSlow]
            public async Task RequestContainsTimeEntryDeletions()
            {
                var timeEntries = createTimeEntries();
                var deletedTimeEntriesCount = timeEntries.Count(t => t.IsDeleted);
                returnsValueOrEmpty(DataSource.TimeEntries, timeEntries);

                var request = await interactor.Execute();

                request.IsEmpty.Should().BeFalse();
                request.TimeEntries.OfType<DeleteAction>().Should().HaveCount(deletedTimeEntriesCount);
            }

            private void returnsValueOrEmpty<TThreadsafe, TDatabase>(IDataSource<TThreadsafe, TDatabase> dataSource, IEnumerable<TThreadsafe> returnValue = null)
                where TDatabase : IDatabaseModel
                where TThreadsafe : TDatabase, IThreadSafeModel
            {
                returnValue ??= Array.Empty<TThreadsafe>();
                dataSource.GetAll(Arg.Any<Func<TDatabase, bool>>(), Arg.Any<bool>()).Returns(Observable.Return(returnValue));
            }

            private void returnsValue<TThreadsafe>(ISingletonDataSource<TThreadsafe> dataSource, TThreadsafe value)
              where TThreadsafe : IThreadSafeModel
            {
                dataSource.Current.Returns(Observable.Return(value));
            }
        }
    }
}
