using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Suggestions;
using Toggl.Core.Sync;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Core.UI.Collections;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.Views;
using Toggl.Core.UI.ViewModels.MainLog;
using Toggl.Core.UI.ViewModels.MainLog.Identity;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage;
using Xunit;
using static Toggl.Core.Helper.Constants;
using ThreadingTask = System.Threading.Tasks.Task;
using Toggl.Core.Exceptions;

namespace Toggl.Core.Tests.UI.ViewModels
{
    using MainLogSection = AnimatableSectionModel<MainLogSectionViewModel, MainLogItemViewModel, IMainLogKey>;

    public sealed class MainViewModelTests
    {
        public abstract class MainViewModelTest : BaseViewModelTests<MainViewModel>
        {
            protected ISubject<Exception> SyncErrorSubject { get; } = new Subject<Exception>();

            protected override MainViewModel CreateViewModel()
            {
                var vm = new MainViewModel(
                    DataSource,
                    SyncManager,
                    TimeService,
                    RatingService,
                    UserPreferences,
                    AnalyticsService,
                    OnboardingStorage,
                    InteractorFactory,
                    NavigationService,
                    RemoteConfigService,
                    AccessibilityService,
                    UpdateRemoteConfigCacheService,
                    AccessRestrictionStorage,
                    SchedulerProvider,
                    RxActionFactory,
                    PermissionsChecker,
                    BackgroundService,
                    PlatformInfo,
                    WidgetsService);

                vm.Initialize();

                return vm;
            }

            protected override void AdditionalSetup()
            {
                base.AdditionalSetup();

                SyncManager.Errors.Returns(SyncErrorSubject.AsObservable());

                var defaultRemoteConfiguration = new RatingViewConfiguration(5, RatingViewCriterion.None);
                RemoteConfigService
                    .GetRatingViewConfiguration()
                    .Returns(defaultRemoteConfiguration);

                DataSource.Preferences.Current.Returns(Observable.Create<IThreadSafePreferences>(observer =>
                {
                    observer.OnNext(new MockPreferences
                    {
                        DateFormat = DateFormat.FromLocalizedDateFormat("dd/mm/YYYY")
                    });
                    return Disposable.Empty;
                }));
            }
        }

        public sealed class TheConstructor : MainViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource,
                bool useSyncManager,
                bool useTimeService,
                bool useRatingService,
                bool useUserPreferences,
                bool useAnalyticsService,
                bool useOnboardingStorage,
                bool useInteractorFactory,
                bool useNavigationService,
                bool useRemoteConfigService,
                bool useAccessibilityService,
                bool useRemoteConfigUpdateService,
                bool useAccessRestrictionStorage,
                bool useSchedulerProvider,
                bool useRxActionFactory,
                bool usePermissionsChecker,
                bool useBackgroundService,
                bool usePlatformInfo,
                bool useWidgetsService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var syncManager = useSyncManager ? SyncManager : null;
                var timeService = useTimeService ? TimeService : null;
                var ratingService = useRatingService ? RatingService : null;
                var userPreferences = useUserPreferences ? UserPreferences : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var remoteConfigService = useRemoteConfigService ? RemoteConfigService : null;
                var accessibilityService = useAccessibilityService ? AccessibilityService : null;
                var remoteConfigUpdateService = useRemoteConfigUpdateService ? UpdateRemoteConfigCacheService : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;
                var accessRestrictionStorage = useAccessRestrictionStorage ? AccessRestrictionStorage : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var permissionsChecker = usePermissionsChecker ? PermissionsChecker : null;
                var backgroundService = useBackgroundService ? BackgroundService : null;
                var platformInfo = usePlatformInfo ? PlatformInfo : null;
                var widgetsService = useWidgetsService ? WidgetsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new MainViewModel(
                        dataSource,
                        syncManager,
                        timeService,
                        ratingService,
                        userPreferences,
                        analyticsService,
                        onboardingStorage,
                        interactorFactory,
                        navigationService,
                        remoteConfigService,
                        accessibilityService,
                        remoteConfigUpdateService,
                        accessRestrictionStorage,
                        schedulerProvider,
                        rxActionFactory,
                        permissionsChecker,
                        backgroundService,
                        platformInfo,
                        widgetsService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheViewAppearingMethod : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToNoWorkspaceViewModelWhenNoWorkspaceStateIsSet()
            {
                AccessRestrictionStorage.HasNoWorkspace().Returns(true);
                SyncErrorSubject.OnNext(new NoWorkspaceException());
                ViewModel.ViewAppearing();

                TestScheduler.Start();

                await NavigationService.Received(1).Navigate<NoWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotNavigateToNoWorkspaceViewModelWhenNoWorkspaceStateIsNotSet()
            {
                SyncErrorSubject.OnNext(new NoWorkspaceException());
                ViewModel.ViewAppearing();
                AccessRestrictionStorage.HasNoWorkspace().Returns(false);

                TestScheduler.Start();

                await NavigationService.DidNotReceive().Navigate<NoWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotNavigateToNoWorkspaceViewSeveralTimes()
            {
                AccessRestrictionStorage.HasNoWorkspace().Returns(true);
                var task = new TaskCompletionSource<Unit>().Task;
                NavigationService.Navigate<NoWorkspaceViewModel, Unit>(View).Returns(task);

                SyncErrorSubject.OnNext(new NoWorkspaceException());
                SyncErrorSubject.OnNext(new NoWorkspaceException());
                ViewModel.ViewAppearing();
                ViewModel.ViewAppearing();
                TestScheduler.Start();

                await NavigationService.Received(1).Navigate<NoWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToSelectDefaultWorkspaceViewModelWhenNoDefaultWorkspaceStateIsSet()
            {
                AccessRestrictionStorage.HasNoDefaultWorkspace().Returns(true);
                SyncErrorSubject.OnNext(new NoDefaultWorkspaceException());
                ViewModel.ViewAppearing();

                TestScheduler.Start();

                await NavigationService.Received().Navigate<SelectDefaultWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotNavigateToSelectDefaultWorkspaceViewModelWhenNoDefaultWorkspaceStateIsNotSet()
            {
                AccessRestrictionStorage.HasNoDefaultWorkspace().Returns(false);
                SyncErrorSubject.OnNext(new Exception());

                TestScheduler.Start();

                await NavigationService.DidNotReceive().Navigate<SelectDefaultWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotNavigateToSelectDefaultWorkspaceViewSeveralTimes()
            {
                AccessRestrictionStorage.HasNoWorkspace().Returns(false);
                AccessRestrictionStorage.HasNoDefaultWorkspace().Returns(true);
                var task = new TaskCompletionSource<Unit>().Task;
                NavigationService.Navigate<SelectDefaultWorkspaceViewModel, Unit>(View).Returns(task);

                SyncErrorSubject.OnNext(new NoDefaultWorkspaceException());
                SyncErrorSubject.OnNext(new NoDefaultWorkspaceException());
                ViewModel.ViewAppearing();
                ViewModel.ViewAppearing();
                TestScheduler.Start();

                await NavigationService.Received(1).Navigate<SelectDefaultWorkspaceViewModel, Unit>(View);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotNavigateToSelectDefaultWorkspaceViewModelWhenTheresNoWorkspaceAvaialable()
            {
                AccessRestrictionStorage.HasNoWorkspace().Returns(true);
                SyncErrorSubject.OnNext(new NoWorkspaceException());
                ViewModel.ViewAppearing();

                TestScheduler.Start();

                await NavigationService.Received().Navigate<NoWorkspaceViewModel, Unit>(View);
                await NavigationService.DidNotReceive().Navigate<SelectDefaultWorkspaceViewModel, Unit>(View);
            }
        }

        public sealed class MainLogCreation : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void EmptyLog()
            {
                var observer = TestScheduler.CreateObserver<IImmutableList<MainLogSection>>();

                Observable.Return(ImmutableList<MainLogSection>.Empty)
                    .MergeToMainLogSections(
                        Observable.Return(ImmutableList<Suggestion>.Empty),
                        Observable.Return(false),
                        null)
                    .Subscribe(observer);

                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                observer.Messages.Count.Should().Be(2);
                observer.LastEmittedValue().Should().BeEquivalentTo(ImmutableList<MainLogSection>.Empty);
            }

            [Fact, LogIfTooSlow]
            public void FeedbackSectionOnly()
            {
                var observer = TestScheduler.CreateObserver<IImmutableList<MainLogSection>>();

                Observable.Return(ImmutableList<MainLogSection>.Empty)
                    .MergeToMainLogSections(
                        Observable.Return(ImmutableList<Suggestion>.Empty),
                        Observable.Return(true),
                        userFeedbackSection)
                    .Subscribe(observer);

                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                observer.Messages.Count.Should().Be(2);
                observer.LastEmittedValue().Should().BeEquivalentTo(ImmutableList.Create(userFeedbackSection));

            }

            [Fact, LogIfTooSlow]
            public void TimeEntriesWithoutSuggestions()
            {
                var observer = TestScheduler.CreateObserver<IImmutableList<MainLogSection>>();

                Observable.Return(timeEntryList).MergeToMainLogSections(Observable.Return(ImmutableList<Suggestion>.Empty), Observable.Return(true), userFeedbackSection)
                    .Subscribe(observer);

                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                var expected = timeEntryList
                    .Prepend(userFeedbackSection);

                observer.Messages.Count.Should().Be(2);
                observer.LastEmittedValue().Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void SuggestionsWithoutTimeEntries()
            {
                var observer = TestScheduler.CreateObserver<IImmutableList<MainLogSection>>();

                var suggestions = Observable.Return(ImmutableList.Create(suggestion));

                Observable.Return(ImmutableList<MainLogSection>.Empty).MergeToMainLogSections(suggestions, Observable.Return(true), userFeedbackSection)
                    .Subscribe(observer);

                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                var expected = ImmutableList.Create(suggestionsSection, userFeedbackSection);

                observer.Messages.Count.Should().Be(2);
                observer.LastEmittedValue().Should().BeEquivalentTo(expected);
            }

            [Fact, LogIfTooSlow]
            public void AllLogItemsTogether()
            {
                var observer = TestScheduler.CreateObserver<IImmutableList<MainLogSection>>();

                var timeEntries = Observable.Return(timeEntryList);
                var suggestions = Observable.Return(ImmutableList.Create(suggestion));
                var shouldShowRatingView = Observable.Return(true);

                timeEntries.MergeToMainLogSections(suggestions, shouldShowRatingView, userFeedbackSection)
                    .Subscribe(observer);

                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                var expected = timeEntryList
                    .Prepend(userFeedbackSection)
                    .Prepend(suggestionsSection);

                observer.Messages.Count.Should().Be(2);
                observer.LastEmittedValue().Should().BeEquivalentTo(expected);
            }

            private static readonly IImmutableList<MainLogSection> timeEntryList = ImmutableList.Create(section1, section2);

            private static readonly Suggestion suggestion =
                new Suggestion(timeEntry, SuggestionProviderType.MostUsedTimeEntries);

            private static readonly MainLogSection suggestionsSection = new MainLogSection(
                new SuggestionsHeaderViewModel(""),
                ImmutableList.Create(new SuggestionLogItemViewModel(0, suggestion)));

            private readonly MainLogSection userFeedbackSection = new MainLogSection(new UserFeedbackViewModel(null), Enumerable.Empty<UserFeedbackViewModel>());

            private static readonly MainLogItemViewModel mainLogItem1 = Substitute.For<MainLogItemViewModel>();
            private static readonly MainLogItemViewModel mainLogItem2 = Substitute.For<MainLogItemViewModel>();
            private static readonly MainLogItemViewModel mainLogItem3 = Substitute.For<MainLogItemViewModel>();
            private static readonly MainLogItemViewModel mainLogItem4 = Substitute.For<MainLogItemViewModel>();

            private static readonly DaySummaryViewModel daySummary1 = new DaySummaryViewModel(DateTime.Now, "First", "1:00");
            private static readonly DaySummaryViewModel daySummary2 = new DaySummaryViewModel(DateTime.Today, "Second", "2:00");

            private static readonly MainLogSection section1 = new MainLogSection(daySummary1, new[] { mainLogItem1, mainLogItem2, mainLogItem3 });
            private static readonly MainLogSection section2 = new MainLogSection(daySummary2, new[] { mainLogItem4 });

            private static IThreadSafeTimeEntry timeEntry
            {
                get
                {
                    var te = Substitute.For<IThreadSafeTimeEntry>();
                    te.Id.Returns(123);
                    return te;
                }
            }
        }

        public sealed class TheStartTimeEntryAction : MainViewModelTest
        {
            private readonly ISubject<IThreadSafeTimeEntry> subject = new Subject<IThreadSafeTimeEntry>();

            public TheStartTimeEntryAction()
            {
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(subject);
                TimeService.CurrentDateTime.Returns(DateTimeOffset.Now);
                ViewModel.Initialize().GetAwaiter().GetResult();

                subject.OnNext(null);
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async ThreadingTask NavigatesToTheStartTimeEntryViewModel(bool isInManualMode, bool useDefaultMode)
            {
                UserPreferences.IsManualModeEnabled.Returns(isInManualMode);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                await NavigationService.Received()
                   .Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(Arg.Any<StartTimeEntryParameters>(), ViewModel.View);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async ThreadingTask PassesTheAppropriatePlaceholderToTheStartTimeEntryViewModel(bool isInManualMode, bool useDefaultMode)
            {
                UserPreferences.IsManualModeEnabled.Returns(isInManualMode);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                var expected = isInManualMode == useDefaultMode
                    ? Resources.ManualTimeEntryPlaceholder
                    : Resources.StartTimeEntryPlaceholder;
                await NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.PlaceholderText == expected),
                    ViewModel.View
                );
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async ThreadingTask PassesTheAppropriateDurationToTheStartTimeEntryViewModel(bool isInManualMode, bool useDefaultMode)
            {
                UserPreferences.IsManualModeEnabled.Returns(isInManualMode);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                var expected = isInManualMode == useDefaultMode
                    ? TimeSpan.FromMinutes(DefaultTimeEntryDurationForManualModeInMinutes)
                    : (TimeSpan?)null;
                await NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.Duration == expected),
                    ViewModel.View
                );
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async ThreadingTask PassesTheAppropriateStartTimeToTheStartTimeEntryViewModel(bool isInManualMode, bool useDefaultMode)
            {
                var date = DateTimeOffset.Now;
                TimeService.CurrentDateTime.Returns(date);
                UserPreferences.IsManualModeEnabled.Returns(isInManualMode);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                var expected = isInManualMode == useDefaultMode
                    ? date.Subtract(TimeSpan.FromMinutes(DefaultTimeEntryDurationForManualModeInMinutes))
                    : date;
                await NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.StartTime == expected),
                    ViewModel.View
                );
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void CannotBeExecutedWhenThereIsARunningTimeEntry(bool useDefaultMode)
            {
                var timeEntry = new MockTimeEntry();
                subject.OnNext(timeEntry);
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                var errors = TestScheduler.CreateObserver<Exception>();
                ViewModel.StartTimeEntry.Errors.Subscribe(errors);
                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();

                errors.Messages.Count.Should().Be(1);
                errors.LastEmittedValue().Should().BeEquivalentTo(new RxActionNotEnabledException());
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void MarksTheActionButtonTappedForOnboardingPurposes(bool useDefaultMode)
            {
                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                OnboardingStorage.Received().StartButtonWasTapped();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async ThreadingTask MarksTheActionNavigatedAwayBeforeStopButtonForOnboardingPurposes(bool useDefaultMode)
            {
                OnboardingStorage.StopButtonWasTappedBefore.Returns(Observable.Return(false));
                await ViewModel.Initialize();
                subject.OnNext(null);
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                OnboardingStorage.DidNotReceive().SetNavigatedAwayFromMainViewAfterStopButton();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async ThreadingTask MarksTheActionNavigatedAwayAfterStopButtonForOnboardingPurposes(bool useDefaultMode)
            {
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                var observable = Observable.Return<IThreadSafeTimeEntry>(null);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);
                OnboardingStorage.StopButtonWasTappedBefore.Returns(Observable.Return(true));
                await ViewModel.Initialize();
                subject.OnNext(null);
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                ViewModel.StartTimeEntry.Execute(useDefaultMode);

                TestScheduler.Start();
                OnboardingStorage.Received().SetNavigatedAwayFromMainViewAfterStopButton();
            }
        }

        public sealed class TheOpenSettingsCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheSettingsViewModel()
            {
                ViewModel.Initialize().Wait();

                ViewModel.OpenSettings.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Navigate<SettingsViewModel>(View);
            }

            [Fact, LogIfTooSlow]
            public void MarksTheActionBeforeStopButtonForOnboardingPurposes()
            {
                OnboardingStorage.StopButtonWasTappedBefore.Returns(Observable.Return(false));
                ViewModel.Initialize().Wait();

                ViewModel.OpenSettings.Execute();

                TestScheduler.Start();
                OnboardingStorage.DidNotReceive().SetNavigatedAwayFromMainViewAfterStopButton();
            }

            [Fact, LogIfTooSlow]
            public void MarksTheActionAfterStopButtonForOnboardingPurposes()
            {
                OnboardingStorage.StopButtonWasTappedBefore.Returns(Observable.Return(true));
                ViewModel.Initialize().Wait();

                ViewModel.OpenSettings.Execute();

                TestScheduler.Start();
                OnboardingStorage.Received().SetNavigatedAwayFromMainViewAfterStopButton();
            }
        }

        public sealed class TheOpenSyncFailuresCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheSyncFailuresViewModel()
            {
                ViewModel.Initialize().Wait();

                ViewModel.OpenSyncFailures.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Navigate<SyncFailuresViewModel>(View);
            }
        }

        public class TheStopTimeEntryAction : MainViewModelTest
        {
            private ISubject<IThreadSafeTimeEntry> subject;

            public TheStopTimeEntryAction()
            {
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                subject = new BehaviorSubject<IThreadSafeTimeEntry>(timeEntry);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(subject);

                ViewModel.Initialize().Wait();
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CallsTheStopMethodOnTheDataSource()
            {
                var date = DateTimeOffset.UtcNow;
                TimeService.CurrentDateTime.Returns(date);

                ViewModel.StopTimeEntry.Execute(TimeEntryStopOrigin.Deeplink);

                TestScheduler.Start();
                await InteractorFactory.Received().StopTimeEntry(date, TimeEntryStopOrigin.Deeplink).Execute();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask InitiatesPushSync()
            {
                ViewModel.StopTimeEntry.Execute(Arg.Any<TimeEntryStopOrigin>());

                TestScheduler.Start();
                SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public void MarksTheActionForOnboardingPurposes()
            {
                ViewModel.StopTimeEntry.Execute(Arg.Any<TimeEntryStopOrigin>());

                TestScheduler.Start();
                OnboardingStorage.Received().StopButtonWasTapped();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotInitiatePushSyncWhenSavingFails()
            {
                InteractorFactory
                    .StopTimeEntry(Arg.Any<DateTimeOffset>(), Arg.Any<TimeEntryStopOrigin>())
                    .Execute()
                    .Returns(ThreadingTask.FromException<IThreadSafeTimeEntry>(new Exception()));

                var errors = TestScheduler.CreateObserver<Exception>();
                ViewModel.StopTimeEntry.Errors.Subscribe(errors);
                ViewModel.StopTimeEntry.Execute(Arg.Any<TimeEntryStopOrigin>());

                TestScheduler.Start();

                errors.Messages.Count().Should().Be(1);
                SyncManager.DidNotReceive().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CannotBeExecutedWhenNoTimeEntryIsRunning()
            {
                subject.OnNext(null);
                TestScheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                var errors = TestScheduler.CreateObserver<Exception>();
                ViewModel.StopTimeEntry.Errors.Subscribe(errors);
                ViewModel.StopTimeEntry.Execute(TimeEntryStopOrigin.Manual);

                TestScheduler.Start();

                errors.Messages.Count.Should().Be(1);
                errors.LastEmittedValue().Should().BeEquivalentTo(new RxActionNotEnabledException());

                await InteractorFactory.DidNotReceive().StopTimeEntry(Arg.Any<DateTimeOffset>(), Arg.Any<TimeEntryStopOrigin>()).Execute();
            }
        }

        public sealed class TheNumberOfSyncFailuresProperty : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTheCountOfInteractorResult()
            {
                var syncables = new IDatabaseSyncable[]
                {
                    new MockTag { Name = "Tag", SyncStatus = SyncStatus.SyncFailed, LastSyncErrorMessage = "Error1" },
                    new MockTag { Name = "Tag2", SyncStatus = SyncStatus.SyncFailed, LastSyncErrorMessage = "Error1" },
                    new MockProject { Name = "Project", SyncStatus = SyncStatus.SyncFailed, LastSyncErrorMessage = "Error2" }
                };
                var items = syncables.Select(i => new SyncFailureItem(i));
                var interactor = Substitute.For<IInteractor<IObservable<IEnumerable<SyncFailureItem>>>>();
                interactor.Execute().Returns(Observable.Return(items));
                InteractorFactory.GetItemsThatFailedToSync().Returns(interactor);
                await ViewModel.Initialize();

                var observer = TestScheduler.CreateObserver<int>();
                ViewModel.NumberOfSyncFailures.Subscribe(observer);
                TestScheduler.AdvanceBy(50);

                observer.Messages
                    .Last(m => m.Value.Kind == System.Reactive.NotificationKind.OnNext).Value.Value
                    .Should()
                    .Be(syncables.Length);
            }
        }

        public abstract class InitialStateTest : MainViewModelTest
        {
            protected void PrepareSuggestion()
            {
                DataSource.TimeEntries.IsEmpty.Returns(Observable.Return(false));
                var suggestionProvider = Substitute.For<ISuggestionProvider>();
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                timeEntry.Id.Returns(123);
                timeEntry.Start.Returns(DateTimeOffset.Now);
                timeEntry.Duration.Returns((long?)null);
                timeEntry.Description.Returns("something");
                var suggestion = new Suggestion(timeEntry, SuggestionProviderType.MostUsedTimeEntries);
                InteractorFactory.GetSuggestions(Arg.Any<int>()).Execute().Returns(Observable.Return(new[] { suggestion }));
            }

            protected void PrepareTimeEntry()
            {
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                timeEntry.Id.Returns(123);
                timeEntry.Start.Returns(DateTimeOffset.Now);
                timeEntry.Duration.Returns(100);
                InteractorFactory.ObserveAllTimeEntriesVisibleToTheUser().Execute()
                    .Returns(Observable.Return(new[] { timeEntry }));
            }

            protected void PrepareIsWelcome(bool isWelcome)
            {
                var subject = new BehaviorSubject<bool>(isWelcome);
                OnboardingStorage.IsNewUser.Returns(subject.AsObservable());
            }
        }

        public sealed class TheShouldShowEmptyStateProperty : InitialStateTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTrueWhenThereAreNoSuggestionsAndNoTimeEntriesAndIsWelcome()
            {
                PrepareIsWelcome(true);
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowEmptyState.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeSuggestions()
            {
                PrepareSuggestion();
                var viewModel = CreateViewModel();
                await viewModel.Initialize();

                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowEmptyState.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeTimeEntries()
            {
                PrepareTimeEntry();
                var viewModel = CreateViewModel();
                await viewModel.Initialize();

                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowEmptyState.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenIsNotWelcome()
            {
                PrepareIsWelcome(false);
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowEmptyState.Subscribe(observer);

                TestScheduler.Start();

                observer.LastEmittedValue().Should().BeFalse();
            }
        }

        public sealed class TheShouldShowWelcomeBackProperty : InitialStateTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTrueWhenThereAreNoSuggestionsAndNoTimeEntriesAndIsNotWelcome()
            {
                PrepareIsWelcome(false);
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowWelcomeBack.Subscribe(observer);

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false),
                    ReactiveTest.OnNext(3, true)
                );
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeSuggestions()
            {
                PrepareSuggestion();
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowWelcomeBack.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeTimeEntries()
            {
                PrepareTimeEntry();
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowWelcomeBack.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenIsWelcome()
            {
                PrepareIsWelcome(true);
                var viewModel = CreateViewModel();
                await viewModel.Initialize();
                var observer = TestScheduler.CreateObserver<bool>();

                viewModel.ShouldShowWelcomeBack.Subscribe(observer);

                TestScheduler.Start();
                observer.LastEmittedValue().Should().BeFalse();
            }
        }

        public sealed class TheInitializeMethod : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async void ReportsUserIdToAnalytics()
            {
                var userId = 1234567890L;
                var user = Substitute.For<IThreadSafeUser>();
                user.Id.Returns(userId);
                InteractorFactory.GetCurrentUser().Execute().Returns(Observable.Return(user));
                await ViewModel.Initialize();

                AnalyticsService.Received().SetUserId(userId);
            }

            public sealed class WhenShowingTheRatingsView : MainViewModelTest
            {
                [Fact, LogIfTooSlow]
                public async void DoesNotShowTheRatingViewByDefault()
                {
                    await ViewModel.Initialize();

                    var observer = TestScheduler.CreateObserver<bool>();
                    ViewModel.ShouldShowRatingView.Subscribe(observer);

                    TestScheduler.Start();
                    observer.LastEmittedValue().Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public async void ShowsTheRatingView()
                {
                    var defaultRemoteConfiguration = new RatingViewConfiguration(5, RatingViewCriterion.Start);
                    RemoteConfigService
                        .GetRatingViewConfiguration()
                        .Returns(defaultRemoteConfiguration);

                    var now = DateTimeOffset.Now;
                    var firstOpened = now - TimeSpan.FromDays(5);

                    TimeService.CurrentDateTime.Returns(now);
                    OnboardingStorage.GetFirstOpened().Returns(firstOpened);

                    await ViewModel.Initialize();
                    var observer = TestScheduler.CreateObserver<bool>();
                    ViewModel.ShouldShowRatingView.Subscribe(observer);

                    TestScheduler.Start();
                    observer.LastEmittedValue().Should().BeTrue();
                }

                [Fact, LogIfTooSlow]
                public async void DoesNotShowTheRatingViewIfThereWasAnInteraction()
                {
                    var defaultRemoteConfiguration = new RatingViewConfiguration(5, RatingViewCriterion.Start);
                    RemoteConfigService
                        .GetRatingViewConfiguration()
                        .Returns(defaultRemoteConfiguration);

                    var now = DateTimeOffset.Now;
                    var firstOpened = now - TimeSpan.FromDays(6);

                    TimeService.CurrentDateTime.Returns(now);
                    OnboardingStorage.GetFirstOpened().Returns(firstOpened);
                    OnboardingStorage.RatingViewOutcome().Returns(RatingViewOutcome.AppWasNotRated);

                    await ViewModel.Initialize();

                    var observer = TestScheduler.CreateObserver<bool>();
                    ViewModel.ShouldShowRatingView.Subscribe(observer);

                    TestScheduler.Start();
                    observer.LastEmittedValue().Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public async void DoesNotShowTheRatingViewIfAfter24HourSnooze()
                {
                    var defaultRemoteConfiguration = new RatingViewConfiguration(5, RatingViewCriterion.Start);
                    RemoteConfigService
                        .GetRatingViewConfiguration()
                        .Returns(defaultRemoteConfiguration);

                    var now = DateTimeOffset.Now;
                    var firstOpened = now - TimeSpan.FromDays(6);
                    var lastInteraction = now - TimeSpan.FromDays(2);

                    TimeService.CurrentDateTime.Returns(now);
                    OnboardingStorage.GetFirstOpened().Returns(firstOpened);
                    OnboardingStorage.RatingViewOutcome().Returns(RatingViewOutcome.AppWasNotRated);
                    OnboardingStorage.RatingViewOutcomeTime().Returns(lastInteraction);

                    await ViewModel.Initialize();

                    var observer = TestScheduler.CreateObserver<bool>();
                    ViewModel.ShouldShowRatingView.Subscribe(observer);

                    TestScheduler.Start();
                    observer.LastEmittedValue().Should().BeFalse();
                }

                [Theory, LogIfTooSlow]
                [InlineData(ApplicationInstallLocation.Internal, Platform.Giskard, true)]
                [InlineData(ApplicationInstallLocation.External, Platform.Giskard, true)]
                [InlineData(ApplicationInstallLocation.Unknown, Platform.Giskard, true)]
                [InlineData(ApplicationInstallLocation.Internal, Platform.Daneel, false)]
                [InlineData(ApplicationInstallLocation.External, Platform.Daneel, false)]
                [InlineData(ApplicationInstallLocation.Unknown, Platform.Daneel, false)]
                public async void TracksApplicationInstallLocation(ApplicationInstallLocation location, Platform platform, bool shouldTrack)
                {
                    PlatformInfo.InstallLocation.Returns(location);
                    PlatformInfo.Platform.Returns(platform);

                    await ViewModel.Initialize();
                    TestScheduler.Start();

                    if (shouldTrack)
                        AnalyticsService.ApplicationInstallLocation.Received().Track(location);
                    else
                        AnalyticsService.ApplicationInstallLocation.DidNotReceive().Track(location);
                }
            }

            [Fact]
            public async void StartsTheWidgetsService()
            {
                await ViewModel.Initialize();
                WidgetsService.Received().Start();
            }
        }
    }
}
