using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Services;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Storage;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Tests.Services
{
    public sealed class UnsyncedDataPersistenceServiceTests
    {
        public abstract class UnsyncedDataPersistenceServiceTest
        {
            protected IAnalyticsService AnalyticsService { get; }
            protected Func<string, StreamWriter, Task> WriteToFile { get; }
            protected IInteractor<Task<UnsyncedDataDump>> CreateUnsyncedDataDumpInteractor { get; }

            public UnsyncedDataPersistenceServiceTest()
            {
                AnalyticsService = Substitute.For<IAnalyticsService>();
                var unsyncedDumpEvent = Substitute.For<IAnalyticsEvent<int, int, int, int>>();
                AnalyticsService.UnsyncedDataDumped.Returns(unsyncedDumpEvent);
                CreateUnsyncedDataDumpInteractor = Substitute.For<IInteractor<Task<UnsyncedDataDump>>>();
                WriteToFile = Substitute.For<Func<string, StreamWriter, Task>>();
            }
        }

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsWhenAnyOfTheArgumentsAreNull(bool useCreateUnsyncedDataDumpInteractor, bool useWriteToFile, bool useAnalyticsService)
            {
                var analyticsService = useAnalyticsService ? Substitute.For<IAnalyticsService>() : null;
                var createUnsyncedDataDumpInteractor = useCreateUnsyncedDataDumpInteractor ? Substitute.For<IInteractor<Task<UnsyncedDataDump>>>() : null;
                var writeToFile = useWriteToFile ? Substitute.For<Func<string, StreamWriter, Task>>() : null;
                Action constructor = () => new UnsyncedDataPersistenceService(
                    analyticsService,
                    writeToFile,
                    createUnsyncedDataDumpInteractor
                );

                constructor.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class ThePersistUnsyncedDataMethod : UnsyncedDataPersistenceServiceTest
        {
            [Fact]
            public async Task ExecutesUnsyncedDataDumpCreation()
            {
                var mockWorkspace = new MockWorkspace();
                CreateUnsyncedDataDumpInteractor.Execute().Returns(
                    new UnsyncedDataDump(
                        new List<IThreadSafeTimeEntry> { new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded) },
                        new List<IThreadSafeTag> { new MockTag(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded) },
                        new List<IThreadSafeProject> { new MockProject(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded) },
                        new List<IThreadSafeClient> { new MockClient(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded) }
                    )
                );

                var unsyncedDataPersistenceService = new UnsyncedDataPersistenceService(AnalyticsService, WriteToFile, CreateUnsyncedDataDumpInteractor);
                await unsyncedDataPersistenceService.PersistUnsyncedData();
                await CreateUnsyncedDataDumpInteractor.Received().Execute();
            }

            [Fact]
            public async Task AttemptsToWriteCorrectDataDump()
            {
                var mockWorkspace = new MockWorkspace();

                CreateUnsyncedDataDumpInteractor.Execute().Returns(
                    new UnsyncedDataDump(
                        new List<IThreadSafeTimeEntry> {new MockTimeEntry(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)},
                        new List<IThreadSafeTag> {new MockTag(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)},
                        new List<IThreadSafeProject> {new MockProject(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)},
                        new List<IThreadSafeClient> {new MockClient(1, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)}
                    )
                );
                var unsyncedDataPersistenceService = new UnsyncedDataPersistenceService(AnalyticsService, WriteToFile, CreateUnsyncedDataDumpInteractor);
                await unsyncedDataPersistenceService.PersistUnsyncedData();
                await WriteToFile.Received().Invoke(SerializedDump, Arg.Any<StreamWriter>());
            }

            [Fact]
            public async Task CallsTheAnalyticsServiceWithTheCorrectAmountsOfSyncedData()
            {
                var mockWorkspace = new MockWorkspace();

                CreateUnsyncedDataDumpInteractor.Execute().Returns(
                    new UnsyncedDataDump(
                        Enumerable.Range(1, 1).Select(id => new MockTimeEntry(id, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)),
                        Enumerable.Range(1, 2).Select(id => new MockTag(id, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)),
                        Enumerable.Range(1, 3).Select(id => new MockProject(id, mockWorkspace, syncStatus: SyncStatus.SyncNeeded)),
                        Enumerable.Range(1, 4).Select(id => new MockClient(id, mockWorkspace, syncStatus: SyncStatus.SyncNeeded))
                    )
                );
                var unsyncedDataPersistenceService = new UnsyncedDataPersistenceService(AnalyticsService, WriteToFile, CreateUnsyncedDataDumpInteractor);
                await unsyncedDataPersistenceService.PersistUnsyncedData();
                AnalyticsService.UnsyncedDataDumped.Received().Track(1, 3, 4, 2);
            }

            private const string SerializedDump = @"{
  ""time_entries"": [
    {
      ""workspace_id"": 0,
      ""project_id"": null,
      ""task_id"": null,
      ""billable"": false,
      ""start"": ""0001-01-01T00:00:00+00:00"",
      ""duration"": null,
      ""description"": null,
      ""tag_ids"": null,
      ""at"": ""0001-01-01T00:00:00+00:00"",
      ""server_deleted_at"": null,
      ""user_id"": 0,
      ""id"": 1,
      ""sync_status"": 1,
      ""last_sync_error_message"": null,
      ""is_deleted"": false,
      ""task"": null,
      ""user"": null,
      ""project"": null,
      ""workspace"": {
        ""name"": null,
        ""admin"": false,
        ""suspended_at"": null,
        ""server_deleted_at"": null,
        ""default_hourly_rate"": null,
        ""default_currency"": null,
        ""only_admins_may_create_projects"": false,
        ""only_admins_see_billable_rates"": false,
        ""only_admins_see_team_dashboard"": false,
        ""projects_billable_by_default"": false,
        ""rounding"": 0,
        ""rounding_minutes"": 0,
        ""at"": ""0001-01-01T00:00:00+00:00"",
        ""logo_url"": null,
        ""id"": 0,
        ""sync_status"": 0,
        ""last_sync_error_message"": null,
        ""is_deleted"": false,
        ""is_inaccessible"": false
      },
      ""tags"": null,
      ""is_inaccessible"": false
    }
  ],
  ""tags"": [
    {
      ""workspace_id"": 0,
      ""name"": null,
      ""at"": ""0001-01-01T00:00:00+00:00"",
      ""server_deleted_at"": null,
      ""id"": 1,
      ""sync_status"": 1,
      ""last_sync_error_message"": null,
      ""is_deleted"": false,
      ""workspace"": {
        ""name"": null,
        ""admin"": false,
        ""suspended_at"": null,
        ""server_deleted_at"": null,
        ""default_hourly_rate"": null,
        ""default_currency"": null,
        ""only_admins_may_create_projects"": false,
        ""only_admins_see_billable_rates"": false,
        ""only_admins_see_team_dashboard"": false,
        ""projects_billable_by_default"": false,
        ""rounding"": 0,
        ""rounding_minutes"": 0,
        ""at"": ""0001-01-01T00:00:00+00:00"",
        ""logo_url"": null,
        ""id"": 0,
        ""sync_status"": 0,
        ""last_sync_error_message"": null,
        ""is_deleted"": false,
        ""is_inaccessible"": false
      },
      ""is_inaccessible"": false
    }
  ],
  ""projects"": [
    {
      ""workspace_id"": 0,
      ""client_id"": null,
      ""name"": null,
      ""is_private"": false,
      ""active"": false,
      ""at"": ""0001-01-01T00:00:00+00:00"",
      ""server_deleted_at"": null,
      ""color"": null,
      ""billable"": null,
      ""template"": null,
      ""auto_estimates"": null,
      ""estimated_hours"": null,
      ""rate"": null,
      ""currency"": null,
      ""actual_hours"": null,
      ""id"": 1,
      ""sync_status"": 1,
      ""last_sync_error_message"": null,
      ""is_deleted"": false,
      ""client"": null,
      ""workspace"": {
        ""name"": null,
        ""admin"": false,
        ""suspended_at"": null,
        ""server_deleted_at"": null,
        ""default_hourly_rate"": null,
        ""default_currency"": null,
        ""only_admins_may_create_projects"": false,
        ""only_admins_see_billable_rates"": false,
        ""only_admins_see_team_dashboard"": false,
        ""projects_billable_by_default"": false,
        ""rounding"": 0,
        ""rounding_minutes"": 0,
        ""at"": ""0001-01-01T00:00:00+00:00"",
        ""logo_url"": null,
        ""id"": 0,
        ""sync_status"": 0,
        ""last_sync_error_message"": null,
        ""is_deleted"": false,
        ""is_inaccessible"": false
      },
      ""tasks"": null,
      ""is_inaccessible"": false
    }
  ],
  ""clients"": [
    {
      ""workspace_id"": 0,
      ""name"": null,
      ""at"": ""0001-01-01T00:00:00+00:00"",
      ""server_deleted_at"": null,
      ""id"": 1,
      ""sync_status"": 1,
      ""last_sync_error_message"": null,
      ""is_deleted"": false,
      ""workspace"": {
        ""name"": null,
        ""admin"": false,
        ""suspended_at"": null,
        ""server_deleted_at"": null,
        ""default_hourly_rate"": null,
        ""default_currency"": null,
        ""only_admins_may_create_projects"": false,
        ""only_admins_see_billable_rates"": false,
        ""only_admins_see_team_dashboard"": false,
        ""projects_billable_by_default"": false,
        ""rounding"": 0,
        ""rounding_minutes"": 0,
        ""at"": ""0001-01-01T00:00:00+00:00"",
        ""logo_url"": null,
        ""id"": 0,
        ""sync_status"": 0,
        ""last_sync_error_message"": null,
        ""is_deleted"": false,
        ""is_inaccessible"": false
      },
      ""is_inaccessible"": false
    }
  ]
}";

        }
    }
}
