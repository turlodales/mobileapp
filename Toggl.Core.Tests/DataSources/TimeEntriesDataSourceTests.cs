using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using NUnit.Framework.Internal;
using Toggl.Core.DataSources;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Shared.Extensions;
using Toggl.Storage;
using Toggl.Storage.Models;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Core.Tests.DataSources
{
    public sealed class TimeEntriesDataSourceTests
    {
        public abstract class TimeEntryDataSourceTest : BaseInteractorTests
        {
            protected const long ProjectId = 10;

            protected const long WorkspaceId = 11;

            protected const long UserId = 12;

            protected const long CurrentRunningId = 13;

            protected const long TaskId = 14;

            protected ITimeEntriesSource TimeEntriesSource { get; }

            protected TestScheduler TestScheduler => SchedulerProvider.TestScheduler;

            protected static DateTimeOffset Now { get; } = new DateTimeOffset(2018, 05, 14, 18, 00, 00, TimeSpan.Zero);

            protected IThreadSafeTimeEntry TimeEntry { get; } =
                Models.TimeEntry.Builder
                    .Create(CurrentRunningId)
                    .SetUserId(UserId)
                    .SetDescription("")
                    .SetWorkspaceId(WorkspaceId)
                    .SetSyncStatus(SyncStatus.InSync)
                    .SetAt(Now.AddDays(-1))
                    .SetStart(Now.AddHours(-2))
                    .Build();

            protected IRepository<IDatabaseTimeEntry> TimeEntriesRepository { get; } = Substitute.For<IRepository<IDatabaseTimeEntry>>();
            protected IRepository<IDatabaseTimeEntry> TimeEntriesBackupRepository { get; } = Substitute.For<IRepository<IDatabaseTimeEntry>>();

            protected TimeEntryDataSourceTest()
            {
                TimeEntriesSource = new TimeEntriesDataSource(
                    TimeEntriesRepository,
                    TimeEntriesBackupRepository,
                    TimeService,
                    AnalyticsService,
                    SchedulerProvider);

                IdProvider.GetNextIdentifier().Returns(-1);
                TimeEntriesRepository.GetById(Arg.Is(TimeEntry.Id)).Returns(Observable.Return(TimeEntry));

                TimeEntriesRepository.Create(Arg.Any<IDatabaseTimeEntry>())
                          .Returns(info => Observable.Return(info.Arg<IDatabaseTimeEntry>()));

                TimeEntriesRepository.Update(Arg.Any<long>(), Arg.Any<IDatabaseTimeEntry>())
                          .Returns(info => Observable.Return(info.Arg<IDatabaseTimeEntry>()));

                TimeService.CurrentDateTime.Returns(Now);
            }
        }

        public sealed class TheConstructor : TimeEntryDataSourceTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useRepository,
                bool useRepositoryBackup,
                bool useTimeService,
                bool useAnalyticsService,
                bool useSchedulerProvider)
            {
                var timeEntriesRepository = useRepository ? TimeEntriesRepository : null;
                var timeEntriesBackupRepository = useRepositoryBackup ? TimeEntriesBackupRepository : null;
                var timeService = useTimeService ? TimeService : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TimeEntriesDataSource(timeEntriesRepository, timeEntriesBackupRepository, timeService, analyticsService, schedulerProvider);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void FixesMultipleRunningTimeEntriesDatabaseInconsistency()
            {
                TimeEntriesRepository.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(
                        Observable.Return(new[]
                        {
                            new MockTimeEntry { Id = 1, Duration = null, IsDeleted = false },
                            new MockTimeEntry { Id = 2, Duration = null, IsDeleted = false },
                        }),
                        Observable.Return(new[]
                        {
                            new MockTimeEntry { Id = 1, Duration = null, IsDeleted = false }
                        }));

                // ReSharper disable once ObjectCreationAsStatement
                new TimeEntriesDataSource(TimeEntriesRepository, TimeEntriesBackupRepository, TimeService, AnalyticsService, SchedulerProvider);

                TimeEntriesRepository.Received().BatchUpdate(
                    Arg.Is<IEnumerable<(long Id, IDatabaseTimeEntry Entity)>>(
                        timeEntries => timeEntries.Count() == 2
                                       && timeEntries.Any(tuple => tuple.Id == 1 && tuple.Entity.Duration == null)
                                       && timeEntries.Any(tuple => tuple.Id == 2 && tuple.Entity.Duration == null)),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Is<IRivalsResolver<IDatabaseTimeEntry>>(rivalsResolver => rivalsResolver != null));
                AnalyticsService.TwoRunningTimeEntriesInconsistencyFixed.Received().Track();
            }

            [Fact]
            public void DoesNotTrackTheEventIfThereAreNotTwoRunningTimeEntries()
            {
                TimeEntriesRepository.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(
                        Observable.Return(new[]
                        {
                            new MockTimeEntry { Id = 1, Duration = null, IsDeleted = false }
                        }));

                // ReSharper disable once ObjectCreationAsStatement
                new TimeEntriesDataSource(TimeEntriesRepository, TimeEntriesBackupRepository, TimeService, AnalyticsService, SchedulerProvider);

                AnalyticsService.TwoRunningTimeEntriesInconsistencyFixed.DidNotReceive().Track();
            }
        }

        public sealed class TheCreateMethod : TimeEntryDataSourceTest
        {
            [Fact]
            public async ThreadingTask CallsRepositoryWithConflictResolvers()
            {
                var timeEntry = new MockTimeEntry();
                TimeEntriesRepository.BatchUpdate(null, null, null)
                    .ReturnsForAnyArgs(Observable.Return(new[] { new CreateResult<IDatabaseTimeEntry>(timeEntry) }));

                await TimeEntriesSource.Create(timeEntry);

                await TimeEntriesRepository.Received().BatchUpdate(
                    Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                    Arg.Is<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(conflictResolution => conflictResolution != null),
                    Arg.Is<IRivalsResolver<IDatabaseTimeEntry>>(resolver => resolver != null));
            }

            [Fact]
            public async ThreadingTask EmitsObservableEventsForTheNewlyCreatedRunningTimeEntry()
            {
                var itemsChangedObserver = TestScheduler.CreateObserver<Unit>();
                var newTimeEntry = new MockTimeEntry { Id = -1, Duration = null };
                TimeEntriesRepository.BatchUpdate(
                    Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>())
                    .Returns(Observable.Return(new IConflictResolutionResult<IDatabaseTimeEntry>[]
                    {
                        new CreateResult<IDatabaseTimeEntry>(newTimeEntry)
                    }));

                var timeEntriesSource = new TimeEntriesDataSource(TimeEntriesRepository, TimeEntriesBackupRepository, TimeService, AnalyticsService, SchedulerProvider);
                timeEntriesSource.ItemsChanged.Subscribe(itemsChangedObserver);
                await timeEntriesSource.Create(newTimeEntry);

                TestScheduler.Start();
                itemsChangedObserver.SingleEmittedValue().Should().Be(Unit.Default);
            }

            [Fact]
            public async ThreadingTask EmitsObservableEventsForTheNewRunningTimeEntryAndTheStoppedTimeEntry()
            {
                var durationAfterStopping = 100;
                var itemsChangedObserver = TestScheduler.CreateObserver<Unit>();
                var createdObserver = TestScheduler.CreateObserver<IThreadSafeTimeEntry>();
                var runningTimeEntry = new MockTimeEntry { Id = 1, Duration = null };
                var newTimeEntry = new MockTimeEntry { Id = -2, Duration = null };
                TimeEntriesRepository.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(Observable.Return(new IDatabaseTimeEntry[] { runningTimeEntry }));
                TimeEntriesRepository.BatchUpdate(
                    Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>())
                    .Returns(Observable.Return(new IConflictResolutionResult<IDatabaseTimeEntry>[]
                    {
                        new UpdateResult<IDatabaseTimeEntry>(runningTimeEntry.Id, runningTimeEntry.With(durationAfterStopping)),
                        new CreateResult<IDatabaseTimeEntry>(newTimeEntry)
                    }));
                var timeEntriesSource = new TimeEntriesDataSource(TimeEntriesRepository, TimeEntriesBackupRepository, TimeService, AnalyticsService, SchedulerProvider);
                timeEntriesSource.ItemsChanged.Subscribe(itemsChangedObserver);

                await timeEntriesSource.Create(newTimeEntry);

                TestScheduler.Start();
                itemsChangedObserver.SingleEmittedValue().Should().Be(Unit.Default);
            }
        }

        public sealed class TheGetAllMethod : TimeEntryDataSourceTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NeverReturnsDeletedTimeEntries()
            {
                var result = Enumerable
                    .Range(0, 20)
                    .Select(i =>
                    {
                        var isDeleted = i % 2 == 0;
                        var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                        timeEntry.Id.Returns(i);
                        timeEntry.IsDeleted.Returns(isDeleted);
                        return timeEntry;
                    });
                DataSource
                    .TimeEntries
                    .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), true)
                    .Returns(callInfo =>
                        Observable
                             .Return(result)
                             .Select(x => x.Where(callInfo.Arg<Func<IThreadSafeTimeEntry, bool>>())));

                var timeEntries = await InteractorFactory.GetAllTimeEntriesVisibleToTheUser().Execute()
                    .Select(tes => tes.Where(x => x.Id > 10));

                timeEntries.Should().HaveCount(5);
            }
        }

        public sealed class TheBatchUpdateMethod : TimeEntryDataSourceTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotReportThatItemsChangedExactlyOnce()

            {
                var observer = TestScheduler.CreateObserver<Unit>();
                TimeEntriesSource.ItemsChanged.Subscribe(observer);
                var timeEntries = Enumerable.Range(0, 10)
                    .Select(id => new MockTimeEntry { Id = id, IsDeleted = false, At = DateTimeOffset.Now, Duration = 1 })
                    .ToList();
                var conflictResolutionResult = Observable.Return(timeEntries.Select(te => new UpdateResult<IDatabaseTimeEntry>(te.Id, te)));
                TimeEntriesRepository.BatchUpdate(
                    Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>())
                    .Returns(conflictResolutionResult);

                await TimeEntriesSource.BatchUpdate(timeEntries);

                TestScheduler.Start();
                observer.Messages.Should().HaveCount(1);
            }
        }
    }
}
