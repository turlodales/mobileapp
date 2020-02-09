using Android.Widget;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AndroidX.RecyclerView.Widget;
using Toggl.Core.Sync;
using Toggl.Core.UI.Onboarding.MainView;
using Toggl.Droid.Adapters;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Helper;
using Toggl.Droid.ViewHolders.MainLog;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Fragments
{
    public sealed partial class MainFragment
    {
        private PopupWindow playButtonTooltipPopupWindow;
        private PopupWindow stopButtonTooltipPopupWindow;
        private PopupWindow tapToEditPopup;

        private readonly BehaviorSubject<int> timeEntriesCountSubject = new BehaviorSubject<int>(0);
        private EditTimeEntryOnboardingStep editTimeEntryOnboardingStep;
        private IDisposable editTimeEntryOnboardingStepDisposable;

        private IObservable<Unit> mainRecyclerViewChangesObservable;
        private ISubject<Unit> mainRecyclerViewScrollChanges = new Subject<Unit>();
        private IDisposable mainRecyclerViewScrollChangesDisposable;

        public override void OnResume()
        {
            base.OnResume();
            mainRecyclerViewScrollChangesDisposable = mainRecyclerView
                .Rx()
                .OnScrolled()
                .ObserveOn(AndroidDependencyContainer.Instance.SchedulerProvider.MainScheduler)
                .Subscribe(mainRecyclerViewScrollChanges.OnNext);
        }

        public override void OnPause()
        {
            base.OnPause();
            mainRecyclerViewScrollChangesDisposable?.Dispose();
        }

        public override void OnStop()
        {
            base.OnStop();
            playButtonTooltipPopupWindow?.Dismiss();
            stopButtonTooltipPopupWindow?.Dismiss();
            tapToEditPopup?.Dismiss();
            playButtonTooltipPopupWindow = null;
            stopButtonTooltipPopupWindow = null;
            tapToEditPopup = null;
        }

        private void setupOnboardingSteps()
        {
            setupMainLogObservables();
            setupStartTimeEntryOnboardingStep();
            setupStopTimeEntryOnboardingStep();
            setupTapToEditOnboardingStep();
        }

        private void setupMainLogObservables()
        {
            var collectionChanges = ViewModel.MainLogItems.SelectUnit();
            mainRecyclerViewChangesObservable = mainRecyclerViewScrollChanges
                .Merge(collectionChanges);
        }

        private void setupStartTimeEntryOnboardingStep()
        {
            if (playButtonTooltipPopupWindow == null)
            {
                playButtonTooltipPopupWindow = PopupWindowFactory.PopupWindowWithText(
                    Context,
                    Resource.Layout.TooltipWithRightArrow,
                    Resource.Id.TooltipText,
                    Shared.Resources.TapToStartTimer);
            }

            new StartTimeEntryOnboardingStep(ViewModel.OnboardingStorage)
                .ManageDismissableTooltip(
                    visibilityChanged,
                    playButtonTooltipPopupWindow,
                    playButton,
                    (popup, anchor) => popup.LeftVerticallyCenteredOffsetsTo(anchor, dpExtraRightMargin: 8),
                    ViewModel.OnboardingStorage)
                .DisposedBy(DisposeBag);
        }

        private void setupStopTimeEntryOnboardingStep()
        {
            if (stopButtonTooltipPopupWindow == null)
            {
                stopButtonTooltipPopupWindow = PopupWindowFactory.PopupWindowWithText(
                    Context,
                    Resource.Layout.TooltipWithRightBottomArrow,
                    Resource.Id.TooltipText,
                    Shared.Resources.TapToStopTimer);
            }

            new StopTimeEntryOnboardingStep(ViewModel.OnboardingStorage, ViewModel.IsTimeEntryRunning)
                .ManageDismissableTooltip(
                    visibilityChanged,
                    stopButtonTooltipPopupWindow,
                    stopButton,
                    (popup, anchor) => popup.TopRightFrom(anchor, dpExtraBottomMargin: 8),
                    ViewModel.OnboardingStorage)
                .DisposedBy(DisposeBag);
        }

        private void setupTapToEditOnboardingStep()
        {
            tapToEditPopup ??= createTapToEditPopup();
            
            editTimeEntryOnboardingStep = new EditTimeEntryOnboardingStep(
                ViewModel.OnboardingStorage, Observable.Return(false));

            var showTapToEditOnboardingStepObservable =
                Observable.CombineLatest(
                    editTimeEntryOnboardingStep.ShouldBeVisible,
                    mainRecyclerViewChangesObservable,
                    ViewModel.SyncProgressState,
                    ViewModel.IsTimeEntryRunning,
                    (shouldShowStep, unit, syncState, isTimeEntryRunning) => shouldShowStep && syncState == SyncProgress.Synced && !isTimeEntryRunning);

            showTapToEditOnboardingStepObservable
                .Where(shouldShowStep => shouldShowStep)
                .Select(_ => findOldestTimeEntryView())
                .ObserveOn(AndroidDependencyContainer.Instance.SchedulerProvider.MainScheduler)
                .Subscribe(updateTapToEditOnboardingStep)
                .DisposedBy(DisposeBag);
        }

        private void updateTapToEditOnboardingStep(MainLogCellViewHolder oldestVisibleTimeEntryViewHolder)
        {
            tapToEditPopup?.Dismiss(); 
            tapToEditPopup ??= createTapToEditPopup();

            if (oldestVisibleTimeEntryViewHolder == null)
                return;

            if (editTimeEntryOnboardingStepDisposable != null)
            {
                editTimeEntryOnboardingStepDisposable.Dispose();
                editTimeEntryOnboardingStepDisposable = null;
            }

            editTimeEntryOnboardingStepDisposable = editTimeEntryOnboardingStep
                .ManageVisibilityOf(
                    visibilityChanged,
                    tapToEditPopup,
                    oldestVisibleTimeEntryViewHolder.ItemView,
                    (window, view) => PopupOffsets.FromDp(16, -4, Context));
        }

        private PopupWindow createTapToEditPopup()
            => PopupWindowFactory.PopupWindowWithText(
                Context,
                Resource.Layout.TooltipWithLeftTopArrow,
                Resource.Id.TooltipText,
                Shared.Resources.TapToEditIt);

        private MainLogCellViewHolder findOldestTimeEntryView()
        {
            if (mainLogRecyclerAdapter == null)
            {
                return null;
            }

            for (var position = layoutManager.FindLastVisibleItemPosition(); position >= 0; position--)
            {
                var viewType = mainLogRecyclerAdapter.GetItemViewType(position);
                if (viewType != MainLogRecyclerAdapter.TimeEntryLogItemViewType)
                {
                    continue;
                }

                var viewHolder = findLogCellViewHolderAtPosition(position);
                if (viewHolder == null)
                    return null;

                if (isFullyVisible(viewHolder))
                {
                    return viewHolder;
                }
            }

            return null;
        }

        private MainLogCellViewHolder findLogCellViewHolderAtPosition(int position)
        {
            var viewHolder = mainRecyclerView.FindViewHolderForLayoutPosition(position);

            if (viewHolder == null)
                return null;

            if (viewHolder is MainLogCellViewHolder logViewHolder)
                return logViewHolder;

            return null;
        }

        private bool isFullyVisible(RecyclerView.ViewHolder view)
        {
            return layoutManager.IsViewPartiallyVisible(view.ItemView, true, true);
        }
    }
}
