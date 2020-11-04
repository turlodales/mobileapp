using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
using Toggl.Core.UI.Collections;
using Toggl.Core.UI.Navigation;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Settings;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Services;
using Toggl.Core.UI.Transformations;
using Toggl.Core.UI.ViewModels.Calendar.ContextualMenu;
using Toggl.Core.UI.ViewModels.Settings;

namespace Toggl.Core.UI.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public sealed class CalendarDayViewModel : ViewModel
    {
        private readonly ITimeService timeService;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IInteractorFactory interactorFactory;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IPermissionsChecker permissionsChecker;

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();
        private readonly IObservable<TimeSpan> timeTrackedOnDaySubject;
        private readonly ISubject<Unit> calendarLinkingCompletedSubject = new Subject<Unit>();

        public DateTimeOffset Date { get; }
        public ObservableGroupedOrderedCollection<CalendarItem> CalendarItems { get; }
        public IObservable<TimeFormat> TimeOfDayFormat { get; }
        public IObservable<DurationFormat> DurationFormat { get; }
        public IObservable<string> TimeTrackedOnDay { get; }
        public IObservable<Unit> CalendarLinkingCompleted => calendarLinkingCompletedSubject.AsObservable();

        public InputAction<CalendarItem> OnTimeEntryEdited { get; }
        public InputAction<(DateTimeOffset, TimeSpan)> OnDurationSelected { get; }
        public CalendarContextualMenuViewModel ContextualMenuViewModel { get; }

        public TrackingOnboardingCondition CalendarTimeEntryTooltipCondition { get; }

        public CalendarDayViewModel(
            DateTimeOffset date,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IRxActionFactory rxActionFactory,
            IUserPreferences userPreferences,
            IAnalyticsService analyticsService,
            IBackgroundService backgroundService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            INavigationService navigationService,
            IOnboardingStorage onboardingStorage,
            IPermissionsChecker permissionsChecker,
            TrackingOnboardingCondition calendarTimeEntryTooltipCondition)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(backgroundService, nameof(backgroundService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(permissionsChecker, nameof(permissionsChecker));
            Ensure.Argument.IsNotNull(calendarTimeEntryTooltipCondition, nameof(calendarTimeEntryTooltipCondition));

            Date = date;
            this.timeService = timeService;
            this.userPreferences = userPreferences;
            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.onboardingStorage = onboardingStorage;
            this.permissionsChecker = permissionsChecker;
            CalendarTimeEntryTooltipCondition = calendarTimeEntryTooltipCondition;

            ContextualMenuViewModel = new CalendarContextualMenuViewModel(
                interactorFactory,
                schedulerProvider,
                analyticsService,
                rxActionFactory,
                timeService,
                navigationService);

            OnTimeEntryEdited = rxActionFactory.FromAsync<CalendarItem>(onTimeEntryEdited);
            OnDurationSelected = rxActionFactory.FromAsync<(DateTimeOffset StartTime, TimeSpan Duration)>(
                tuple => durationSelected(tuple.StartTime, tuple.Duration));

            var preferences = dataSource.Preferences.Current;
            TimeOfDayFormat = preferences
                .Select(current => current.TimeOfDayFormat)
                .AsDriver(schedulerProvider);

            DurationFormat = preferences
                .Select(current => current.DurationFormat)
                .AsDriver(schedulerProvider);

            CalendarItems = new ObservableGroupedOrderedCollection<CalendarItem>(
                indexKey: item => item.StartTime,
                orderingKey: item => item.StartTime,
                groupingKey: _ => 0);

            var durationFormat = preferences.Select(current => current.DurationFormat);
            var selectedCalendarsChanged = userPreferences
                .EnabledCalendars
                .SelectUnit();

            var appResumedFromBackground = backgroundService
                .AppResumedFromBackground
                .SelectUnit();

            Observable.Merge(dataSource.TimeEntries.ItemsChanged, selectedCalendarsChanged, appResumedFromBackground)
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .SelectMany(_ => reloadData())
                .ObserveOn(schedulerProvider.MainScheduler)
                .Subscribe(CalendarItems.ReplaceWith)
                .DisposedBy(disposeBag);

            var timeTrackedToday = interactorFactory.ObserveTimeTrackedToday()
                .Execute()
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .StartWith(TimeSpan.Zero);

            var timeTrackedOnDay = CalendarItems
                .CollectionChange
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .Select(_ => CalendarItems.IsEmpty ? ImmutableList<CalendarItem>.Empty : CalendarItems[0])
                .Select(items => items
                        .Where(item => item.Source == CalendarItemSource.TimeEntry)
                        .Sum(item => item.Duration(timeService.CurrentDateTime)))
                .StartWith(TimeSpan.Zero);

            if (timeService.CurrentDateTime.LocalDateTime.Date == Date)
            {
                timeTrackedOnDaySubject = timeTrackedOnDay
                    .CombineLatest(timeTrackedToday.ReemitWhen(timeService.MidnightObservable.SelectUnit()),
                        selectTrackedTimeSource);
            }
            else
            {
                timeTrackedOnDaySubject = timeTrackedOnDay;
            }

            TimeTrackedOnDay = timeTrackedOnDaySubject
                .CombineLatest(durationFormat, DurationAndFormatToString.Convert)
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            onboardingStorage.SetCalendarTabWasOpened();

            connectCalendars();
        }

        private void connectCalendars()
        {
            if (onboardingStorage.CalendarPermissionWasAskedBefore())
                return;

            if (onboardingStorage.ConnectCalendarsPopupWasShown())
                return;

            var calendarEmptyObservable = CalendarItems.Empty.Throttle(TimeSpan.FromSeconds(1)).Where(isEmpty => isEmpty).SelectUnit();
            var tooltipDismissedObservable = CalendarTimeEntryTooltipCondition.Dismissed.SelectUnit();
            var connectCalendarsObservable = calendarEmptyObservable.Merge(tooltipDismissedObservable);

            connectCalendarsObservable
                .SelectMany(_ => permissionsChecker.CalendarPermissionGranted)
                .Where(permissionGranted => !permissionGranted)
                .Where(_ => !onboardingStorage.ConnectCalendarsPopupWasShown())
                .SelectMany(_ =>
                    Navigate<ConnectCalendarsPopupViewModel, Unit, bool>(Unit.Default)
                        .ToObservable())
                .Do(shouldRequestPermission => { if (!shouldRequestPermission) calendarLinkingCompletedSubject.OnNext(Unit.Default); })
                .Where(shouldRequestPermission => shouldRequestPermission)
                .SelectMany(_ => View.RequestCalendarAuthorization(false))
                .Do(_ => onboardingStorage.SetCalendarPermissionWasAskedBefore())
                .Subscribe(permissionGranted => onCalendarPermission(permissionGranted));
        }

        private void onCalendarPermission(bool permissionGranted)
        {
            if (permissionGranted)
            {
                userPreferences.SetCalendarIntegrationEnabled(true);
                Navigate<IndependentCalendarSettingsViewModel>();
                calendarLinkingCompletedSubject.OnNext(Unit.Default);
            }
            else
            {
                Navigate<CalendarPermissionDeniedViewModel>();
                calendarLinkingCompletedSubject.OnNext(Unit.Default);
            }
        }

        private TimeSpan selectTrackedTimeSource(TimeSpan timeTrackedOnDay, TimeSpan timeTrackedToday)
            => timeService.CurrentDateTime.LocalDateTime.Date == Date
                ? timeTrackedToday
                : timeTrackedOnDay;

        private IObservable<IEnumerable<CalendarItem>> reloadData()
        {
            return interactorFactory.GetCalendarItemsForDate(Date.ToLocalTime().Date).Execute();
        }

        private async Task onTimeEntryEdited(CalendarItem calendarItem)
        {
            if (!calendarItem.IsEditable() || calendarItem.TimeEntryId == null)
                return;

            var timeEntry = await interactorFactory.GetTimeEntryById(calendarItem.TimeEntryId.Value).Execute();

            var dto = new DTOs.EditTimeEntryDto
            {
                Id = timeEntry.Id,
                Description = timeEntry.Description,
                StartTime = calendarItem.StartTime,
                StopTime = calendarItem.EndTime,
                ProjectId = timeEntry.ProjectId,
                TaskId = timeEntry.TaskId,
                Billable = timeEntry.Billable,
                WorkspaceId = timeEntry.WorkspaceId,
                TagIds = timeEntry.TagIds
            };

            var duration = calendarItem.Duration.HasValue
                ? calendarItem.Duration.Value.TotalSeconds
                : (timeService.CurrentDateTime - calendarItem.StartTime.LocalDateTime).TotalSeconds;

            if (timeEntry.Duration != duration)
            {
                analyticsService.TimeEntryChangedFromCalendar.Track(CalendarChangeEvent.Duration);
            }

            if (timeEntry.Start != calendarItem.StartTime)
            {
                analyticsService.TimeEntryChangedFromCalendar.Track(CalendarChangeEvent.StartTime);
            }

            await interactorFactory.UpdateTimeEntry(dto).Execute();
        }

        private async Task durationSelected(DateTimeOffset startTime, TimeSpan duration)
        {
            var workspace = await interactorFactory.GetDefaultWorkspace()
                .TrackException<InvalidOperationException, IThreadSafeWorkspace>("CalendarViewModel.durationSelected")
                .Execute();

            var prototype = duration.AsTimeEntryPrototype(startTime, workspace.Id);
            await interactorFactory.CreateTimeEntry(prototype, TimeEntryStartOrigin.CalendarTapAndDrag).Execute();
        }
    }
}
