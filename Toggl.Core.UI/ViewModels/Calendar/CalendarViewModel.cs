using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Calendar;
using Toggl.Core.DataSources;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Services;
using Toggl.Core.Sync;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.Services;
using Toggl.Core.UI.Transformations;
using Toggl.Core.UI.ViewModels.Settings;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Storage;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public sealed class CalendarViewModel : ViewModel
    {
        private const int availablePastWeeksCount = 8;
        private const int availablePastDaysCount = 7 * availablePastWeeksCount;
        private const int availableFutureWeeksCount = 8;
        private const int availableFutureDaysCount = 7 * availableFutureWeeksCount;
        private const string dateFormat = "dddd, MMM d";

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IBackgroundService backgroundService;
        private readonly IInteractorFactory interactorFactory;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IPermissionsChecker permissionsChecker;
        private readonly IRxActionFactory rxActionFactory;
        private readonly ISyncManager syncManager;

        private readonly ISubject<Unit> realoadWeekView = new Subject<Unit>();

        public IObservable<string> CurrentlyShownDateString { get; }

        public BehaviorRelay<DateTime> CurrentlyShownDate { get; }

        public IObservable<ImmutableList<CalendarWeeklyViewDayViewModel>> WeekViewDays { get; }

        public IObservable<ImmutableList<DayOfWeek>> WeekViewHeaders { get; }

        public ViewAction OpenSettings { get; }

        public InputAction<CalendarWeeklyViewDayViewModel> SelectDayFromWeekView { get; }

        public IObservable<IThreadSafeTimeEntry> CurrentRunningTimeEntry { get; private set; }
        public IObservable<string> ElapsedTime { get; private set; }
        public IObservable<bool> IsTimeEntryRunning { get; private set; }
        public IObservable<bool> IsInManualMode { get; private set; }

        public InputAction<bool> StartTimeEntry { get; private set; }

        public ViewAction StopTimeEntry { get; private set; }

        public InputAction<EditTimeEntryInfo> SelectTimeEntry { get; private set; }

        public TrackingOnboardingCondition CalendarTimeEntryTooltipCondition { get; private set; }

        public CalendarViewModel(
            ITogglDataSource dataSource,
            ITimeService timeService,
            IRxActionFactory rxActionFactory,
            IUserPreferences userPreferences,
            IAnalyticsService analyticsService,
            IBackgroundService backgroundService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            IOnboardingStorage onboardingStorage,
            IPermissionsChecker permissionsChecker,
            ISyncManager syncManager,
            INavigationService navigationService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(syncManager, nameof(syncManager));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(backgroundService, nameof(backgroundService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(permissionsChecker, nameof(permissionsChecker));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.syncManager = syncManager;
            this.rxActionFactory = rxActionFactory;
            this.userPreferences = userPreferences;
            this.analyticsService = analyticsService;
            this.backgroundService = backgroundService;
            this.interactorFactory = interactorFactory;
            this.schedulerProvider = schedulerProvider;
            this.onboardingStorage = onboardingStorage;
            this.permissionsChecker = permissionsChecker;

            OpenSettings = rxActionFactory.FromAsync(openSettings);
            SelectDayFromWeekView = rxActionFactory.FromAction<CalendarWeeklyViewDayViewModel>(selectDayFromWeekView);

            var beginningOfWeekObservable = dataSource.User.Current.Select(user => user.BeginningOfWeek);

            WeekViewDays = beginningOfWeekObservable
                .ReemitWhen(timeService.MidnightObservable.SelectUnit())
                .Select(weekViewDays)
                .Select(days => days.ToImmutableList())
                .AsDriver(schedulerProvider);

            WeekViewHeaders = beginningOfWeekObservable
                .ReemitWhen(realoadWeekView)
                .Select(weekViewHeaders)
                .Select(dayOfWeekHeaders => dayOfWeekHeaders.ToImmutableList())
                .AsDriver(schedulerProvider);

            CurrentlyShownDate = new BehaviorRelay<DateTime>(timeService.CurrentDateTime.ToLocalTime().Date);

            CurrentlyShownDateString = CurrentlyShownDate.AsObservable()
                .DistinctUntilChanged()
                .Select(date => DateTimeToFormattedString.Convert(date, dateFormat))
                .AsDriver(schedulerProvider);

            CalendarTimeEntryTooltipCondition = new OnboardingCondition(
                OnboardingConditionKey.CalendarTimeEntryTooltip,
                onboardingStorage,
                schedulerProvider,
                Observable.Return(true))
                .TrackingDismissEvents(analyticsService);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            CurrentRunningTimeEntry = dataSource.TimeEntries
                .CurrentlyRunningTimeEntry
                .AsDriver(schedulerProvider);

            var durationObservable = dataSource
                .Preferences
                .Current
                .Select(preferences => preferences.DurationFormat);

            ElapsedTime = timeService
                .CurrentDateTimeObservable
                .CombineLatest(CurrentRunningTimeEntry, (now, te) => (now - te?.Start) ?? TimeSpan.Zero)
                .CombineLatest(durationObservable, (duration, format) => duration.ToFormattedString(format))
                .AsDriver(schedulerProvider);

            IsTimeEntryRunning = dataSource.TimeEntries
                .CurrentlyRunningTimeEntry
                .Select(te => te != null)
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            IsInManualMode = userPreferences
                .IsManualModeEnabledObservable
                .AsDriver(schedulerProvider);

            StartTimeEntry = rxActionFactory.FromAsync<bool>(startTimeEntry, IsTimeEntryRunning.Invert());
            StopTimeEntry = rxActionFactory.FromObservable(stopTimeEntry, IsTimeEntryRunning);
            SelectTimeEntry = rxActionFactory.FromAsync<EditTimeEntryInfo>(timeEntrySelected);
        }

        public override void ViewDestroyed()
        {
            base.ViewDestroyed();

            disposeBag.Dispose();
        }

        public void RealoadWeekView()
            => realoadWeekView.OnNext(Unit.Default);

        private IEnumerable<DayOfWeek> weekViewHeaders(BeginningOfWeek beginningOfWeek)
        {
            var currentDay = beginningOfWeek.ToDayOfWeekEnum();
            for (int i = 0; i < 7; i++)
            {
                yield return currentDay;
                currentDay = currentDay.NextEnumValue();
            }
        }

        private IEnumerable<CalendarWeeklyViewDayViewModel> weekViewDays(BeginningOfWeek beginningOfWeek)
        {
            var now = timeService.CurrentDateTime.ToLocalTime();
            var today = now.Date;
            var firstAvailableDate = now.AddDays(-availablePastDaysCount + 1).Date;
            var lastAvailableDate = now.AddDays(availableFutureDaysCount).Date;
            var firstShownDate = firstAvailableDate.BeginningOfWeek(beginningOfWeek).Date;
            var lastShownDate = lastAvailableDate.BeginningOfWeek(beginningOfWeek).Date;

            var currentDate = firstShownDate;
            while (currentDate != lastShownDate)
            {
                var dateIsViewable = currentDate <= lastAvailableDate && currentDate >= firstAvailableDate;
                yield return new CalendarWeeklyViewDayViewModel(currentDate, currentDate == today, dateIsViewable);
                currentDate = currentDate.AddDays(1);
            }
        }

        public CalendarDayViewModel DayViewModelAt(int index)
        {
            var currentDate = timeService.CurrentDateTime.ToLocalTime().Date;
            var date = currentDate.AddDays(index);
            return new CalendarDayViewModel(
                date,
                timeService,
                dataSource,
                rxActionFactory,
                userPreferences,
                analyticsService,
                backgroundService,
                interactorFactory,
                schedulerProvider,
                NavigationService,
                onboardingStorage,
                permissionsChecker,
                CalendarTimeEntryTooltipCondition);
        }

        public DateTime IndexToDate(int index)
        {
            var today = timeService.CurrentDateTime.ToLocalTime().Date;
            return today.AddDays(index);
        }

        private Task openSettings()
            => Navigate<SettingsViewModel>();

        private void selectDayFromWeekView(CalendarWeeklyViewDayViewModel day)
        {
            CurrentlyShownDate.Accept(day.Date);

            var daysSinceToday = (timeService.CurrentDateTime.ToLocalTime().Date - day.Date).Days;
            var dayOfWeek = day.Date.DayOfWeek.ToString();
            analyticsService.CalendarWeeklyDatePickerSelectionChanged.Track(daysSinceToday, dayOfWeek);
        }

        private Task startTimeEntry(bool useDefaultMode)
        {
            var initializeInManualMode = useDefaultMode == userPreferences.IsManualModeEnabled;

            var requestCameFromLongPress = !useDefaultMode;
            var parameter = initializeInManualMode
                ? StartTimeEntryParameters.ForManualMode(timeService.CurrentDateTime, requestCameFromLongPress)
                : StartTimeEntryParameters.ForTimerMode(timeService.CurrentDateTime, requestCameFromLongPress);

            return Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(parameter);
        }

        private IObservable<Unit> stopTimeEntry()
        {
            return interactorFactory
                .StopTimeEntry(timeService.CurrentDateTime, TimeEntryStopOrigin.Manual)
                .Execute()
                .ToObservable()
                .Do(syncManager.InitiatePushSync)
                .SelectUnit();
        }

        private async Task timeEntrySelected(EditTimeEntryInfo editTimeEntryInfo)
        {
            analyticsService.EditViewOpened.Track(editTimeEntryInfo.Origin);
            await Navigate<EditTimeEntryViewModel, long[]>(editTimeEntryInfo.Ids);
        }
    }
}
