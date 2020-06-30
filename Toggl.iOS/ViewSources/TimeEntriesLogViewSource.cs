using Foundation;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Core.UI.Collections;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.ViewModels.MainLog;
using Toggl.Core.UI.ViewModels.MainLog.Identity;
using Toggl.iOS.Cells.MainLog;
using Toggl.iOS.Extensions;
using Toggl.iOS.Views;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewSources
{
    using MainLogSection = AnimatableSectionModel<MainLogSectionViewModel, MainLogItemViewModel, IMainLogKey>;

    public sealed class TimeEntriesLogViewSource
        : BaseTableViewSource<MainLogSection, MainLogSectionViewModel, MainLogItemViewModel>
    {
        private const float suggestionsSeparation = 12;
        private const int rowHeightCompact = 64;
        private const int rowHeightRegular = 48;
        private const int headerHeight = 48;
        private const int userFeedbackHeight = 245;

        public delegate IObservable<DaySummaryViewModel> ObservableHeaderForSection(int section);

        private readonly Subject<TimeEntryLogItemViewModel> continueTapSubject = new Subject<TimeEntryLogItemViewModel>();
        private readonly Subject<TimeEntryLogItemViewModel> continueSwipeSubject = new Subject<TimeEntryLogItemViewModel>();
        private readonly Subject<TimeEntryLogItemViewModel> deleteSwipeSubject = new Subject<TimeEntryLogItemViewModel>();
        private readonly Subject<GroupId> toggleGroupExpansionSubject = new Subject<GroupId>();

        private readonly Subject<UITableViewCell> willDisplayCellSubject = new Subject<UITableViewCell>();
        private readonly Subject<bool> isDraggingSubject = new Subject<bool>();

        private bool swipeActionsEnabled = true;

        public const int SpaceBetweenSections = 20;

        public IObservable<UITableViewCell> WillDisplayCell { get; }
        public IObservable<TimeEntryLogItemViewModel> ContinueTap { get; }
        public IObservable<TimeEntryLogItemViewModel> SwipeToContinue { get; }
        public IObservable<TimeEntryLogItemViewModel> SwipeToDelete { get; }
        public IObservable<GroupId> ToggleGroupExpansion { get; }

        public IObservable<bool> IsDragging { get; }

        public TimeEntriesLogViewSource()
        {
            if (!NSThread.Current.IsMainThread)
            {
                throw new InvalidOperationException($"{nameof(TimeEntriesLogViewSource)} must be created on the main thread");
            }

            ContinueTap = continueTapSubject.AsObservable();
            SwipeToContinue = continueSwipeSubject.AsObservable();
            SwipeToDelete = deleteSwipeSubject.AsObservable();
            WillDisplayCell = willDisplayCellSubject.AsObservable();
            ToggleGroupExpansion = toggleGroupExpansionSubject.AsObservable();

            IsDragging = isDraggingSubject.AsObservable();
        }

        public void SetSwipeActionsEnabled(bool enabled)
        {
            swipeActionsEnabled = enabled;
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            var viewModel = HeaderOf((int) section);

            switch (viewModel)
            {
                case SuggestionsHeaderViewModel _:
                    return headerHeight;
                case DaySummaryViewModel _:
                    return firstDaySummarySectionIndex == (int)section
                        ? headerHeight + SpaceBetweenSections / 2 // The suggestions cell already include a space below
                        : headerHeight + SpaceBetweenSections;
            }

            return 0;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            => getHeightForRow(tableView, indexPath);

        // It needs this method, otherwise the ContentOffset will reset to 0 everytime the table reloads. ¯\_(ツ)_/¯
        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            => getHeightForRow(tableView, indexPath);

        private nfloat getHeightForRow(UITableView tableView, NSIndexPath indexPath) {
            var viewModel = ModelAt(indexPath);

            switch (viewModel)
            {
                case SuggestionLogItemViewModel _:
                    return suggestionsSeparation + (tableView.TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular
                               ? rowHeightRegular
                               : rowHeightCompact);

                case UserFeedbackViewModel _:
                    return userFeedbackHeight;

                case TimeEntryLogItemViewModel _:
                    return tableView.TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular
                        ? rowHeightRegular
                        : rowHeightCompact;
            }

            return 0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var viewModel = ModelAt(indexPath);

            switch (viewModel)
            {
                case SuggestionLogItemViewModel suggestionLogItemViewModel:
                    var suggestionCell = (SuggestionLogViewCell) tableView.DequeueReusableCell(SuggestionLogViewCell.Identifier);
                    suggestionCell.Item = suggestionLogItemViewModel;
                    suggestionCell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    return suggestionCell;

                case UserFeedbackViewModel userFeedbackViewModel:
                    var userFeedbackCell = (UserFeedbackTableViewCell) tableView.DequeueReusableCell(UserFeedbackTableViewCell.Identifier);
                    userFeedbackCell.Item = userFeedbackViewModel;
                    userFeedbackCell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    return userFeedbackCell;

                case TimeEntryLogItemViewModel timeEntryLogItemViewModel:
                    var timeEntryCell = (TimeEntriesLogViewCell) tableView.DequeueReusableCell(TimeEntriesLogViewCell.Identifier);
                    configureTimeEntryLogViewCell(timeEntryCell, timeEntryLogItemViewModel, indexPath);
                    return timeEntryCell;

                default:
                    throw new InvalidCastException($"Cannot configure cell for view model of type {viewModel.GetType().Name}");
            }
        }

        private void configureTimeEntryLogViewCell(TimeEntriesLogViewCell cell, TimeEntryLogItemViewModel viewModel, NSIndexPath indexPath)
        {
            cell.ContinueButtonTap
                .Subscribe(() => continueTapSubject.OnNext(viewModel))
                .DisposedBy(cell.DisposeBag);

            cell.ToggleGroup
                .Subscribe(() => toggleGroupExpansionSubject.OnNext(viewModel.GroupId))
                .DisposedBy(cell.DisposeBag);

            cell.Item = viewModel;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var viewModel = HeaderOf((int) section);
            switch (viewModel)
            {
                case SuggestionsHeaderViewModel suggestionsHeaderViewModel:
                    var suggestionsHeader = (SuggestionsHeaderView)tableView.DequeueReusableHeaderFooterView(SuggestionsHeaderView.Identifier);
                    suggestionsHeader.Item = suggestionsHeaderViewModel;
                    return suggestionsHeader;

                case UserFeedbackSectionViewModel _:
                    return null;

                case DaySummaryViewModel daySummaryViewModel:
                    var daySummaryHeader = (TimeEntriesLogHeaderView)tableView.DequeueReusableHeaderFooterView(TimeEntriesLogHeaderView.Identifier);
                    daySummaryHeader.Item = viewModel as DaySummaryViewModel;
                    return daySummaryHeader;

                default:
                    throw new InvalidCastException($"Cannot configure header for view model of type {viewModel.GetType().Name}");
            }
        }

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            var viewModel = ModelAt(indexPath);
            if (viewModel is TimeEntryLogItemViewModel && swipeActionsEnabled)
            {
                return createSwipeActionConfiguration(continueSwipeActionFor, indexPath);
            }

            return createDisabledActionConfiguration();
        }

        public override UISwipeActionsConfiguration GetTrailingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            var viewModel = ModelAt(indexPath);
            if (viewModel is TimeEntryLogItemViewModel && swipeActionsEnabled)
            {
                return createSwipeActionConfiguration(deleteSwipeActionFor, indexPath);
            }

            return createDisabledActionConfiguration();
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            isDraggingSubject.OnNext(true);
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            isDraggingSubject.OnNext(false);
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            willDisplayCellSubject.OnNext(cell);
        }

        private UISwipeActionsConfiguration createSwipeActionConfiguration(
            Func<TimeEntryLogItemViewModel, UIContextualAction> factory, NSIndexPath indexPath)
        {
            var viewModel = ModelAt(indexPath);
            if (viewModel != null && viewModel is TimeEntryLogItemViewModel)
                return UISwipeActionsConfiguration.FromActions(new[] { factory(viewModel as TimeEntryLogItemViewModel) });

            return null;
        }

        private UIContextualAction continueSwipeActionFor(TimeEntryLogItemViewModel viewModel)
        {
            var continueAction = UIContextualAction.FromContextualActionStyle(
                UIContextualActionStyle.Normal,
                Resources.Continue,
                (action, sourceView, completionHandler) =>
                {
                    continueSwipeSubject.OnNext(viewModel);
                    completionHandler.Invoke(finished: true);
                }
            );
            continueAction.BackgroundColor = Core.UI.Helper.Colors.TimeEntriesLog.ContinueSwipeActionBackground.ToNativeColor();
            return continueAction;
        }

        private UIContextualAction deleteSwipeActionFor(TimeEntryLogItemViewModel viewModel)
        {
            var deleteAction = UIContextualAction.FromContextualActionStyle(
                UIContextualActionStyle.Destructive,
                Resources.Delete,
                (action, sourceView, completionHandler) =>
                {
                    deleteSwipeSubject.OnNext(viewModel);
                    completionHandler.Invoke(finished: true);
                }
            );
            deleteAction.BackgroundColor = Core.UI.Helper.Colors.TimeEntriesLog.DeleteSwipeActionBackground.ToNativeColor();
            return deleteAction;
        }

        private UISwipeActionsConfiguration createDisabledActionConfiguration()
        {
            var swipeAction = UISwipeActionsConfiguration.FromActions(new UIContextualAction[]{});
            swipeAction.PerformsFirstActionWithFullSwipe = false;
            return swipeAction;
        }

        private int firstDaySummarySectionIndex => Sections.IndexOf(section => section.Header is DaySummaryViewModel);
    }
}
