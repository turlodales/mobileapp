using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Services;
using Toggl.Core.UI.ViewModels.DateRangePicker;
using Toggl.Core.UI.Views;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using DateRangeSelectionResult = Toggl.Core.UI.ViewModels.DateRangePicker.DateRangePickerViewModel.DateRangeSelectionResult;

namespace Toggl.Core.UI.ViewModels.Reports
{
    public sealed class ReportsViewModel : ViewModel
    {
        private const string downArrowCharacter = "▾";
        private long? selectedWorkspaceId;
        private Either<DateRangePeriod, DateRange> selection;

        private bool shouldReloadReportOnViewAppeared;
        private readonly ISubject<Unit> reloadReportsSubject = new Subject<Unit>();

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly IInteractorFactory interactorFactory;
        private readonly IDateRangeShortcutsService dateRangeShortcutsService;

        private readonly ITimeService timeService;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;
        private readonly ISubject<IThreadSafeWorkspace> workspaceSelectedById = new Subject<IThreadSafeWorkspace>();

        public IObservable<IImmutableList<IReportElement>> Elements { get; private set; }
        public IObservable<bool> HasMultipleWorkspaces { get; }
        public IObservable<string> CurrentWorkspaceName { get; private set; }

        public IObservable<string> FormattedDateRange { get; private set; }

        public IObservable<DateRange> DateRange { get; private set; }

        public OutputAction<IThreadSafeWorkspace> SelectWorkspace { get; }
        public OutputAction<DateRangeSelectionResult> SelectDateRange { get; }
        public InputAction<DateRangeSelectionResult> SetDateRange { get; }
        public ViewAction OpenYourPlanView { get; }

        public ReportsViewModel(
            ITogglDataSource dataSource,
            INavigationService navigationService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory,
            IAnalyticsService analyticsService,
            ITimeService timeService,
            IDateRangeShortcutsService dateRangeShortcutsService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dateRangeShortcutsService, nameof(dateRangeShortcutsService));

            this.dataSource = dataSource;
            this.interactorFactory = interactorFactory;
            this.schedulerProvider = schedulerProvider;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.dateRangeShortcutsService = dateRangeShortcutsService;

            HasMultipleWorkspaces = interactorFactory.ObserveAllWorkspaces().Execute()
                .Select(workspaces => workspaces.Where(w => !w.IsInaccessible))
                .Select(w => w.Count() > 1)
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            SelectWorkspace = rxActionFactory.FromAsync(selectWorkspace);
            SelectDateRange = rxActionFactory.FromAsync(selectDateRange);
            OpenYourPlanView = rxActionFactory.FromAsync(openYourPlanView);
            SetDateRange = rxActionFactory.FromAction<DateRangeSelectionResult>(setDateRange);
        }

        public override async Task Initialize()
        {
            var workspaceSelector = interactorFactory.GetDefaultWorkspace().Execute()
                .Concat(SelectWorkspace.Elements.WhereNotNull())
                .Merge(workspaceSelectedById.AsObservable());

            var initialSelection = new DateRangeSelectionResult(
                    dateRangeShortcutsService.GetShortcutFrom(DateRangePeriod.ThisWeek).DateRange,
                    DateRangeSelectionSource.Initial);

            var dateRangeSelector = SelectDateRange.Elements
                .Merge(SetDateRange.Inputs)
                .StartWith(initialSelection)
                .WhereNotNull();

            CurrentWorkspaceName = workspaceSelector
                .Select(ws => ws.Name)
                .StartWith("")
                .DistinctUntilChanged()
                .AsDriver("", schedulerProvider);

            dataSource.TimeEntries
                .ItemsChanged
                .Subscribe(_ => shouldReloadReportOnViewAppeared = true)
                .DisposedBy(disposeBag);

            Elements = reloadReportsSubject.StartWith(Unit.Default).CombineLatest(
                    workspaceSelector,
                    dateRangeSelector,
                    (_, workspace, dateRange) => ReportProcessData.Create(workspace, dateRange))
                .SelectMany(reportElements)
                .AsDriver(ImmutableList<IReportElement>.Empty, schedulerProvider);

            var dateFormatObservable = dataSource.Preferences
                .Current
                .Select(preferences => preferences.DateFormat);

            DateRange = dateRangeSelector
                .Select(dateRangeSelectionResult => dateRangeSelectionResult.SelectedRange)
                .WhereNotNull()
                .AsDriver(schedulerProvider);

            FormattedDateRange = DateRange
                .CombineLatest(dateFormatObservable, resultSelector: formattedDateRange)
                .DistinctUntilChanged()
                .Select(dateRange => $"{dateRange} {downArrowCharacter}")
                .AsDriver("", schedulerProvider);

            selectedWorkspaceId = (await interactorFactory.GetDefaultWorkspace().Execute())?.Id;

            selection = Either<DateRangePeriod, DateRange>.WithLeft(DateRangePeriod.ThisWeek);
        }

        public override void ViewDestroyed()
        {
            base.ViewDestroyed();

            disposeBag.Dispose();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (shouldReloadReportOnViewAppeared)
                reloadReportsSubject.OnNext(Unit.Default);
        }

        public async Task SelectWorkspaceById(long id)
        {
            var allWorkspaces = await interactorFactory.GetAllWorkspaces().Execute();
            var ws = allWorkspaces.FirstOrDefault(ws => !ws.IsInaccessible && ws.Id == id);
            if (ws == null) return;
            selectedWorkspaceId = ws.Id;
            workspaceSelectedById.OnNext(ws);
        }

        private async Task openYourPlanView()
        {
            var currentPlan = await interactorFactory.ObserveCurrentWorkspacePlan()
                .Execute()
                .FirstAsync();

            if (currentPlan == Plan.Free)
            {
                await Navigate<YourPlanViewModel>();
            }
        }

        private async Task<IThreadSafeWorkspace> selectWorkspace()
        {
            var allWorkspaces = await interactorFactory.GetAllWorkspaces().Execute();

            var accessibleWorkspaces = allWorkspaces
                .Where(ws => !ws.IsInaccessible)
                .Select(ws => new SelectOption<IThreadSafeWorkspace>(ws, ws.Name))
                .ToImmutableList();

            var currentWorkspaceIndex = accessibleWorkspaces.IndexOf(w => w.Item.Id == selectedWorkspaceId);

            var workspace = await View.Select(Resources.SelectWorkspace, accessibleWorkspaces, currentWorkspaceIndex);

            if (workspace == null || workspace.Id == selectedWorkspaceId)
                return null;

            selectedWorkspaceId = workspace.Id;

            return workspace;
        }

        private async Task<DateRangeSelectionResult> selectDateRange()
        {
            var dateRangeSelection = await Navigate<DateRangePickerViewModel, Either<DateRangePeriod, DateRange>, DateRangeSelectionResult>(selection);
            if (dateRangeSelection?.SelectedRange == null)
                return null;

            selection = Either<DateRangePeriod, DateRange>.WithRight(dateRangeSelection.SelectedRange.Value);

            return dateRangeSelection;
        }

        private void setDateRange(DateRangeSelectionResult result)
        {
            selection = Either<DateRangePeriod, DateRange>.WithRight(result.SelectedRange.Value);
        }

        private ImmutableList<IReportElement> createLoadingStateReportElements()
            => elements(
                ReportSummaryElement.LoadingState,
                ReportBarChartElement.LoadingState,
                ReportDonutChartDonutElement.LoadingState);

        private IObservable<ImmutableList<IReportElement>> reportElements(ReportProcessData processData)
            => reportElementsProcess(processData)
            .ToObservable()
            .StartWith(createLoadingStateReportElements());

        private string formattedDateRange(DateRange dateRange, DateFormat dateFormat)
        {
            var knownShortcut = dateRangeShortcutsService.GetShortcutFrom(dateRange);
            if (knownShortcut != null)
                return knownShortcut.Text;

            var startDateText = dateRange.Beginning.ToString(dateFormat.Short, CultureInfo.InvariantCulture);
            var endDateText = dateRange.End.ToString(dateFormat.Short, CultureInfo.InvariantCulture);
            return $"{startDateText} - {endDateText}";
        }

        private async Task<ImmutableList<IReportElement>> reportElementsProcess(ReportProcessData reportProcessData)
        {
            var analyticsData = new ReportsAnalyticsEventData();

            try
            {
                var filter = reportProcessData.Filter;
                analyticsData.StartTimestamp = timeService.CurrentDateTime.UtcDateTime;
                analyticsData.Source = reportProcessData.SelectionSource;
                analyticsData.TotalDays = (filter.TimeRange.Maximum - filter.TimeRange.Minimum).Days + 1;
                analyticsData.IsSuccessful = true;

                var user = await interactorFactory.GetCurrentUser().Execute();

                var reportsTotal = await interactorFactory
                    .GetReportsTotals(user.Id, filter.Workspace.Id, filter.TimeRange)
                    .Execute();

                var summaryData = await interactorFactory
                    .GetProjectSummary(filter.Workspace.Id, filter.TimeRange.Minimum, filter.TimeRange.Maximum)
                    .Execute();

                analyticsData.ProjectsNotSyncedCount = summaryData.ProjectsNotSyncedCount;

                var preferences = await interactorFactory
                    .ObserveCurrentPreferences()
                    .Execute()
                    .FirstAsync();

                var durationFormat = preferences.DurationFormat;
                var dateFormat = preferences.DateFormat;

                var currentPlan = await interactorFactory
                    .ObserveCurrentWorkspacePlan()
                    .Execute()
                    .FirstAsync();

                var barChartElement = summaryData.Segments.None()
                    ? new ReportProjectsBarChartPlaceholderElement()
                    : (IReportElement)new ReportProjectsBarChartElement(reportsTotal, dateFormat);

                return elements(
                    new ReportWorkspaceNameElement(filter.Workspace.Name),
                    new ReportSummaryElement(summaryData, durationFormat),
                    barChartElement,
                    new ReportAdvancedReportsViaWebElement(currentPlan),
                    new ReportProjectsDonutChartElement(summaryData, durationFormat));
            }
            catch (Exception ex)
            {
                analyticsData.IsSuccessful = false;
                return elements(new ReportErrorElement(ex));
            }
            finally
            {
                shouldReloadReportOnViewAppeared = false;
                trackReportElementProcessCompletion(analyticsData);
            }
        }

        private ImmutableList<IReportElement> elements(params IReportElement[] elements)
            => elements.Flatten();

        private void trackReportElementProcessCompletion(ReportsAnalyticsEventData eventData)
        {
            var loadingTime = timeService.CurrentDateTime.UtcDateTime - eventData.StartTimestamp;

            if (eventData.IsSuccessful)
            {
                analyticsService.ReportsSuccess.Track(eventData.Source, eventData.TotalDays, eventData.ProjectsNotSyncedCount, loadingTime.TotalMilliseconds);
            }
            else
            {
                analyticsService.ReportsFailure.Track(eventData.Source, eventData.TotalDays, loadingTime.TotalMilliseconds);
            }
        }

        private struct ReportsAnalyticsEventData
        {
            public DateTime StartTimestamp { get; set; }
            public DateRangeSelectionSource Source { get; set; }
            public int TotalDays { get; set; }
            public int ProjectsNotSyncedCount { get; set; }
            public bool IsSuccessful { get; set; }
        }
    }
}
