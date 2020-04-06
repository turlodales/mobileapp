using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Interactors;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Storage;
using Toggl.Storage.Models;
using Xunit;

namespace Toggl.Core.Tests.Interactors.Sync
{
    public class CreateUnsyncedDataDumpInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource)
            {
                var dataSource = useDataSource ? DataSource : null;
                Action tryingToConstructWithEmptyParameters =
                    () => new CreateUnsyncedDataDumpInteractor(dataSource);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            public void Setup(
                List<IThreadSafeTimeEntry> timeEntries = null,
                List<IThreadSafeTag> tags = null,
                List<IThreadSafeProject> projects = null,
                List<IThreadSafeClient> clients = null)
            {
                var timeEntriesToReturn = timeEntries ?? new List<IThreadSafeTimeEntry>();
                var tagsToReturn = tags ?? new List<IThreadSafeTag>();
                var projectsToReturn = projects ?? new List<IThreadSafeProject>();
                var clientsToReturn = clients ?? new List<IThreadSafeClient>();

                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(callInfo => Observable.Return(timeEntriesToReturn.Where<IThreadSafeTimeEntry>(
                        callInfo.Arg<Func<IDatabaseTimeEntry, bool>>())));

                DataSource.Tags.GetAll(Arg.Any<Func<IDatabaseTag, bool>>())
                    .Returns(callInfo => Observable.Return(tagsToReturn.Where<IThreadSafeTag>(
                        callInfo.Arg<Func<IDatabaseTag, bool>>())));

                DataSource.Projects.GetAll(Arg.Any<Func<IDatabaseProject, bool>>())
                    .Returns(callInfo => Observable.Return(projectsToReturn.Where<IThreadSafeProject>(
                        callInfo.Arg<Func<IDatabaseProject, bool>>())));

                DataSource.Clients.GetAll(Arg.Any<Func<IDatabaseClient, bool>>())
                    .Returns(callInfo => Observable.Return(clientsToReturn.Where<IThreadSafeClient>(
                        callInfo.Arg<Func<IDatabaseClient, bool>>())));
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotReturnSyncedEntities()
            {
                var mockWorkspace = new MockWorkspace();
                Setup(
                    new List<IThreadSafeTimeEntry> {new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeTag> {new MockTag(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeProject> {new MockProject(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeClient> {new MockClient(1, mockWorkspace, syncStatus: SyncStatus.InSync)}
                );

                var dumpData = await InteractorFactory.CreateUnsyncedDataDump().Execute();

                dumpData.TimeEntries.Should().BeEmpty();
                dumpData.Tags.Should().BeEmpty();
                dumpData.Projects.Should().BeEmpty();
                dumpData.Clients.Should().BeEmpty();
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsUnsyncedTimeEntries()
            {
                var mockWorkspace = new MockWorkspace();
                Setup(
                    new List<IThreadSafeTimeEntry>
                    {
                        new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded),
                        new MockTimeEntry(2, mockWorkspace, syncStatus: SyncStatus.SyncFailed),
                        new MockTimeEntry(3, mockWorkspace, syncStatus: SyncStatus.RefetchingNeeded),
                    },
                    new List<IThreadSafeTag> {new MockTag(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeProject> {new MockProject(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeClient> {new MockClient(1, mockWorkspace, syncStatus: SyncStatus.InSync)}
                );

                var dumpData = await InteractorFactory.CreateUnsyncedDataDump().Execute();

                dumpData.TimeEntries.Should().HaveCount(3);
                dumpData.TimeEntries
                    .Select(te => (id: te.Id, syncStatus: te.SyncStatus))
                    .Should().ContainInOrder(
                        (id: 1L, syncStatus: SyncStatus.SyncNeeded),
                        (id: 2L, syncStatus: SyncStatus.SyncFailed),
                        (id: 3L, syncStatus: SyncStatus.RefetchingNeeded));
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsUnsyncedTags()
            {
                var mockWorkspace = new MockWorkspace();
                Setup(
                    new List<IThreadSafeTimeEntry> {new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.InSync),},
                    new List<IThreadSafeTag>
                    {
                        new MockTag(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded),
                        new MockTag(2, mockWorkspace, syncStatus: SyncStatus.SyncFailed),
                        new MockTag(3, mockWorkspace, syncStatus: SyncStatus.RefetchingNeeded),
                    },
                    new List<IThreadSafeProject> {new MockProject(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeClient> {new MockClient(1, mockWorkspace, syncStatus: SyncStatus.InSync)}
                );

                var dumpData = await InteractorFactory.CreateUnsyncedDataDump().Execute();

                dumpData.Tags.Should().HaveCount(3);
                dumpData.Tags.Select(te => (id: te.Id, syncStatus: te.SyncStatus))
                    .Should().ContainInOrder(
                        (id: 1L, syncStatus: SyncStatus.SyncNeeded),
                        (id: 2L, syncStatus: SyncStatus.SyncFailed),
                        (id: 3L, syncStatus: SyncStatus.RefetchingNeeded));
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsUnsyncedProjects()
            {
                var mockWorkspace = new MockWorkspace();
                Setup(
                    new List<IThreadSafeTimeEntry> {new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeTag> {new MockTag(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeProject>
                    {
                        new MockProject(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded),
                        new MockProject(2, mockWorkspace, syncStatus: SyncStatus.SyncFailed),
                        new MockProject(3, mockWorkspace, syncStatus: SyncStatus.RefetchingNeeded),
                    },
                    new List<IThreadSafeClient> {new MockClient(1, mockWorkspace, syncStatus: SyncStatus.InSync)}
                );

                var dumpData = await InteractorFactory.CreateUnsyncedDataDump().Execute();

                dumpData.Projects.Should().HaveCount(3);
                dumpData.Projects
                    .Select(te => (id: te.Id, syncStatus: te.SyncStatus))
                    .Should().ContainInOrder(
                        (id: 1L, syncStatus: SyncStatus.SyncNeeded),
                        (id: 2L, syncStatus: SyncStatus.SyncFailed),
                        (id: 3L, syncStatus: SyncStatus.RefetchingNeeded));
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsUnsyncedClients()
            {
                var mockWorkspace = new MockWorkspace();
                Setup(
                    new List<IThreadSafeTimeEntry> {new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeTag> {new MockTag(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeProject> {new MockProject(1, mockWorkspace, syncStatus: SyncStatus.InSync)},
                    new List<IThreadSafeClient>
                    {
                        new MockClient(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded),
                        new MockClient(2, mockWorkspace, syncStatus: SyncStatus.SyncFailed),
                        new MockClient(3, mockWorkspace, syncStatus: SyncStatus.RefetchingNeeded),
                    }
                );

                var dumpData = await InteractorFactory.CreateUnsyncedDataDump().Execute();

                dumpData.Clients.Should().HaveCount(3);
                dumpData.Clients
                    .Select(te => (id: te.Id, syncStatus: te.SyncStatus))
                    .Should().ContainInOrder(
                        (id: 1L, syncStatus: SyncStatus.SyncNeeded),
                        (id: 2L, syncStatus: SyncStatus.SyncFailed),
                        (id: 3L, syncStatus: SyncStatus.RefetchingNeeded));
            }
        }
    }
}