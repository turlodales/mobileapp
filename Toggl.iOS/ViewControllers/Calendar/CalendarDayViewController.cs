using System.Reactive.Linq;
using System;
using CoreGraphics;
using Toggl.Core;
using Toggl.Core.Calendar;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.iOS.Presentation;
using Toggl.iOS.Views.Calendar;
using Toggl.iOS.ViewSources;
using Toggl.Shared.Extensions;
using UIKit;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using Foundation;
using Remotion.Linq.Utilities;
using Toggl.Core.Analytics;
using Toggl.Core.Extensions;
using Toggl.Core.Services;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.ViewModels.Calendar.ContextualMenu;
using Toggl.iOS.Cells.Calendar;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.iOS.Views;
using Toggl.Shared;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Storage;

namespace Toggl.iOS.ViewControllers
{
    public sealed partial class CalendarDayViewController : ReactiveViewController<CalendarDayViewModel>, IScrollableToTop
    {
        private const double minimumOffsetOfCurrentTimeIndicatorFromScreenEdge = 0.2;
        private const double middleOfTheDay = 12;
        private const float collectionViewDefaultInset = 20;
        private const float additionalContentOffsetWhenContextualMenuIsVisible = 128;

        private readonly ITimeService timeService;
        private readonly IRxActionFactory rxActionFactory;

        private bool contextualMenuInitialised;

        private CalendarCollectionViewLayout layout;
        private CalendarCollectionViewSource dataSource;
        private CalendarCollectionViewEditItemHelper editItemHelper;
        private CalendarCollectionViewCreateFromSpanHelper createFromSpanHelper;
        private CalendarCollectionViewZoomHelper zoomHelper;
        private CalendarCollectionViewContextualMenuDismissHelper tapToDismissHelper;

        public float ScrollOffset => (float)CalendarCollectionView.ContentOffset.Y;

        private readonly BehaviorRelay<bool> contextualMenuVisible;
        private readonly BehaviorRelay<nfloat> runningTimeEntryCardHeight;
        private readonly BehaviorRelay<string> timeTrackedOnDay;
        private readonly BehaviorRelay<int> currentPageRelay;

        private UIView calendarTimeEntryTooltip;

        public CalendarDayViewController(CalendarDayViewModel viewModel,
            BehaviorRelay<int> currentPageRelay,
            BehaviorRelay<string> timeTrackedOnDay,
            BehaviorRelay<bool> contextualMenuVisible,
            BehaviorRelay<nfloat> runningTimeEntryCardHeight)
            : base(viewModel, nameof(CalendarDayViewController))
        {
            Ensure.Argument.IsNotNull(ViewModel, nameof(ViewModel));
            Ensure.Argument.IsNotNull(currentPageRelay, nameof(currentPageRelay));
            Ensure.Argument.IsNotNull(timeTrackedOnDay, nameof(timeTrackedOnDay));
            Ensure.Argument.IsNotNull(contextualMenuVisible, nameof(contextualMenuVisible));
            Ensure.Argument.IsNotNull(runningTimeEntryCardHeight, nameof(runningTimeEntryCardHeight));

            timeService = IosDependencyContainer.Instance.TimeService;
            rxActionFactory = IosDependencyContainer.Instance.RxActionFactory;

            this.currentPageRelay = currentPageRelay;
            this.timeTrackedOnDay = timeTrackedOnDay;
            this.contextualMenuVisible = contextualMenuVisible;
            this.runningTimeEntryCardHeight = runningTimeEntryCardHeight;
        }

        public void SetScrollOffset(float scrollOffset)
        {
            CalendarCollectionView?.SetContentOffset(new CGPoint(0, scrollOffset), false);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.ContextualMenuViewModel.AttachView(this);

            ContextualMenu.Layer.CornerRadius = 8;
            ContextualMenu.Layer.ShadowColor = UIColor.Black.CGColor;
            ContextualMenu.Layer.ShadowOpacity = 0.1f;
            ContextualMenu.Layer.ShadowOffset = new CGSize(0, -2);

            ContextualMenuBottonConstraint.Constant = -ContextualMenu.Frame.Height - 10;

            ContextualMenuFadeView.FadeLeft = true;
            ContextualMenuFadeView.FadeRight = true;

            dataSource = new CalendarCollectionViewSource(
                timeService,
                CalendarCollectionView,
                ViewModel.TimeOfDayFormat,
                ViewModel.DurationFormat,
                ViewModel.CalendarItems);

            layout = new CalendarCollectionViewLayout(ViewModel.Date, timeService, dataSource);

            editItemHelper = new CalendarCollectionViewEditItemHelper(CalendarCollectionView, timeService, rxActionFactory, dataSource, layout);
            createFromSpanHelper = new CalendarCollectionViewCreateFromSpanHelper(CalendarCollectionView, dataSource, layout);
            zoomHelper = new CalendarCollectionViewZoomHelper(CalendarCollectionView, layout);
            tapToDismissHelper = new CalendarCollectionViewContextualMenuDismissHelper(CalendarCollectionView, dataSource);

            CalendarCollectionView.SetCollectionViewLayout(layout, false);
            CalendarCollectionView.Delegate = dataSource;
            CalendarCollectionView.DataSource = dataSource;

            //Editing items
            dataSource.ItemTapped
                .Select(item => (CalendarItem?)item)
                .Subscribe(ViewModel.ContextualMenuViewModel.OnCalendarItemUpdated.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.CalendarItemInEditMode
                .Subscribe(editItemHelper.StartEditingItem.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.MenuVisible
                .Where(isVisible => !isVisible)
                .SelectUnit()
                .Subscribe(editItemHelper.StopEditing.Inputs)
                .DisposedBy(DisposeBag);

            editItemHelper.ItemUpdated
                .Subscribe(dataSource.UpdateItemView)
                .DisposedBy(DisposeBag);

            editItemHelper.ItemUpdated
                .Select(item => (CalendarItem?)item)
                .Subscribe(ViewModel.ContextualMenuViewModel.OnCalendarItemUpdated.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.DiscardChanges
                .Subscribe(_ => editItemHelper.DiscardChanges())
                .DisposedBy(DisposeBag);

            //Contextual menu
            ViewModel.ContextualMenuViewModel.CurrentMenu
                .Select(menu => menu.Actions)
                .Subscribe(replaceContextualMenuActions)
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.MenuVisible
                .Where(isVisible => isVisible)
                .Subscribe(_ => showContextualMenu())
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.MenuVisible
                .Where(isVisible => !isVisible)
                .Subscribe(_ => dismissContextualMenu())
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.CalendarItemRemoved
                .Subscribe(dataSource.RemoveItemView)
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.CalendarItemUpdated
                .Subscribe(dataSource.UpdateItemView)
                .DisposedBy(DisposeBag);

            ContextualMenuCloseButton.Rx().Tap()
                .Subscribe(_ => ViewModel.ContextualMenuViewModel.OnCalendarItemUpdated.Execute(null))
                .DisposedBy(DisposeBag);

            tapToDismissHelper.DidTapOnEmptySpace
                .Subscribe(_ => ViewModel.ContextualMenuViewModel.OnCalendarItemUpdated.Execute(null))
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.TimeEntryPeriod
                .Subscribe(ContextualMenuTimeEntryPeriodLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.TimeEntryInfo
                .Select(timeEntryInfo => timeEntryInfo.ToAttributedString(ContextualMenuTimeEntryDescriptionProjectTaskClientLabel.Font.CapHeight))
                .Subscribe(ContextualMenuTimeEntryDescriptionProjectTaskClientLabel.Rx().AttributedText())
                .DisposedBy(DisposeBag);

            ViewModel.ContextualMenuViewModel.TimeEntryInfo
                .Subscribe(_ => ViewModel.CalendarTimeEntryTooltipCondition.Dismiss())
                .DisposedBy(DisposeBag);

            ViewModel.TimeTrackedOnDay
                .ReemitWhen(currentPageRelay.SelectUnit())
                .Subscribe(notifyTotalDurationIfCurrentPage)
                .DisposedBy(DisposeBag);

            runningTimeEntryCardHeight
                .Subscribe(_ => updateContentInset())
                .DisposedBy(DisposeBag);

            CalendarCollectionView.LayoutIfNeeded();

            dataSource.WillDisplayCellObservable
                .CombineLatest(ViewModel.CalendarTimeEntryTooltipCondition.ConditionMet, (cell, conditionMet) => (cell, conditionMet))
                .Subscribe(tuple => showCalendarTimeEntryTooltipIfNecessary(tuple.cell, tuple.conditionMet));

            ViewModel.ContextualMenuViewModel.TimeEntryInfo
                .Subscribe(_ => removeCalendarTimeEntryTooltip(TooltipDismissReason.ConditionMet))
                .DisposedBy(DisposeBag);
        }

        private void showCalendarTimeEntryTooltipIfNecessary(UICollectionViewCell cell, bool shouldShow)
        {
            if (!shouldShow)
            {
                calendarTimeEntryTooltip?.RemoveFromSuperview();
                calendarTimeEntryTooltip = null;
                return;
            }

            if (calendarTimeEntryTooltip != null)
                return;

            if (cell is CalendarItemView calendarItemView&& calendarItemView.Item.Source != CalendarItemSource.TimeEntry)
                return;

            var (tooltip, tooltipPointsUpwards, tooltipArrow) = createCalendarEntryTooltip(cell);
            calendarTimeEntryTooltip = tooltip;

            calendarTimeEntryTooltip.Rx()
                .Tap()
                .Subscribe(_ => ViewModel.CalendarTimeEntryTooltipCondition.Dismiss())
                .DisposedBy(DisposeBag);

            View.AddSubview(tooltip);

            var cellHasHorizontalNeighbours = cell.Frame.Right < CalendarCollectionView.Frame.Right - 16;
            if (cellHasHorizontalNeighbours)
                tooltipArrow.LeadingAnchor.ConstraintEqualTo(tooltip.LeadingAnchor, 14).Active = true;
            else
                tooltip.CenterXAnchor.ConstraintEqualTo(tooltipArrow.CenterXAnchor).Active = true;

            if (tooltipPointsUpwards)
                tooltip.TopAnchor.ConstraintEqualTo(cell.BottomAnchor).Active = true;
            else
                tooltip.BottomAnchor.ConstraintEqualTo(cell.TopAnchor).Active = true;

            tooltip.WidthAnchor.ConstraintEqualTo(220).Active = true;
            tooltipArrow.CenterXAnchor.ConstraintEqualTo(cell.CenterXAnchor).Active = true;
        }

        private void removeCalendarTimeEntryTooltip(TooltipDismissReason reason)
        {
            if (calendarTimeEntryTooltip == null) return;
            calendarTimeEntryTooltip.RemoveFromSuperview();
            calendarTimeEntryTooltip = null;
            IosDependencyContainer.Instance.AnalyticsService.TooltipDismissed.Track(OnboardingConditionKey.CalendarTimeEntryTooltip, reason);
        }

        private (UIView tooltip, bool tooltipPointsUpwards, TriangleView tooltipArrow) createCalendarEntryTooltip(UICollectionViewCell cell)
        {
            var arrowHeight = 8;
            var arrowWidth = 16;
            var closeImageSize = 24;

            var tooltip = new UIView();
            tooltip.TranslatesAutoresizingMaskIntoConstraints = false;

            var background = new UIView();
            background.BackgroundColor = ColorAssets.DarkAccent;
            background.Layer.CornerRadius = 8;
            background.TranslatesAutoresizingMaskIntoConstraints = false;

            var arrow = new TriangleView();
            arrow.Color = ColorAssets.DarkAccent;
            arrow.BackgroundColor = UIColor.Clear;
            arrow.TranslatesAutoresizingMaskIntoConstraints = false;

            var messageLabel = new UILabel();

            messageLabel.Lines = 0;
            messageLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            messageLabel.Font = UIFont.SystemFontOfSize(15, UIFontWeight.Semibold);
            var attributes = new UIStringAttributes()
            {
                ParagraphStyle = new NSMutableParagraphStyle()
                {
                    LineSpacing = 6
                },
                ForegroundColor = ColorAssets.AlwaysWhite
            };
            var messasgeString = new NSMutableAttributedString(Resources.HereIsYourTimeEntryInCalendarView);
            messasgeString.AddAttributes(attributes, new NSRange(0, messasgeString.Length));
            messageLabel.AttributedText = messasgeString;

            var gotItLabel = new UILabel();
            gotItLabel.Text = Resources.OkGotIt;
            gotItLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            gotItLabel.Font = UIFont.SystemFontOfSize(15, UIFontWeight.Semibold);
            gotItLabel.TextColor = ColorAssets.AlwaysWhite;

            var closeImage = new UIImageView();
            closeImage.Image = UIImage.FromBundle("x").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            closeImage.TintColor = UIColor.White;

            closeImage.ContentMode = UIViewContentMode.Center;
            closeImage.TranslatesAutoresizingMaskIntoConstraints = false;

            tooltip.AddSubview(arrow);
            tooltip.AddSubview(background);
            background.AddSubview(gotItLabel);
            background.AddSubview(closeImage);
            background.AddSubview(messageLabel);

            //Background
            background.LeadingAnchor.ConstraintEqualTo(tooltip.LeadingAnchor).Active = true;
            background.TrailingAnchor.ConstraintEqualTo(tooltip.TrailingAnchor).Active = true;

            var verticalSpaceNeeded = 150;
            var verticalSpaceavailable = CalendarCollectionView.ContentSize.Height - cell.Frame.Bottom;
            var tooltipPointsUpwards = verticalSpaceavailable >= verticalSpaceNeeded;
            if (tooltipPointsUpwards)
            {
                arrow.Direction = TriangleView.TriangleDirection.Up;
                arrow.TopAnchor.ConstraintEqualTo(tooltip.TopAnchor).Active = true;
                background.TopAnchor.ConstraintEqualTo(arrow.BottomAnchor).Active = true;
                background.BottomAnchor.ConstraintEqualTo(tooltip.BottomAnchor).Active = true;
            }
            else
            {
                arrow.Direction = TriangleView.TriangleDirection.Down;
                background.TopAnchor.ConstraintEqualTo(tooltip.TopAnchor).Active = true;
                background.BottomAnchor.ConstraintEqualTo(arrow.TopAnchor).Active = true;
                arrow.BottomAnchor.ConstraintEqualTo(tooltip.BottomAnchor).Active = true;
            }

            //Arrow
            arrow.WidthAnchor.ConstraintEqualTo(arrowWidth).Active = true;
            arrow.HeightAnchor.ConstraintEqualTo(arrowHeight).Active = true;

            //Message
            messageLabel.TopAnchor.ConstraintEqualTo(background.TopAnchor, 12).Active = true;
            messageLabel.LeadingAnchor.ConstraintEqualTo(background.LeadingAnchor, 14).Active = true;
            messageLabel.TrailingAnchor.ConstraintEqualTo(background.TrailingAnchor, -28).Active = true;
            messageLabel.BottomAnchor.ConstraintEqualTo(gotItLabel.TopAnchor, -10).Active = true;

            //Got it
            gotItLabel.TrailingAnchor.ConstraintEqualTo(background.TrailingAnchor, -22).Active = true;
            gotItLabel.BottomAnchor.ConstraintEqualTo(background.BottomAnchor, -10).Active = true;

            //Close icon
            closeImage.TrailingAnchor.ConstraintEqualTo(background.TrailingAnchor).Active = true;
            closeImage.TopAnchor.ConstraintEqualTo(background.TopAnchor).Active = true;
            closeImage.WidthAnchor.ConstraintEqualTo(closeImageSize).Active = true;
            closeImage.HeightAnchor.ConstraintEqualTo(closeImageSize).Active = true;

            return (tooltip, tooltipPointsUpwards, arrow);
        }

        private void notifyTotalDurationIfCurrentPage(string durationString)
        {
            if (currentPageRelay.Value == View.Tag)
            {
                timeTrackedOnDay.Accept(durationString);
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            updateContentInset();
            layout.InvalidateLayoutForVisibleItems();

            if (contextualMenuInitialised) return;
            contextualMenuInitialised = true;
            ContextualMenuBottonConstraint.Constant = -ContextualMenu.Frame.Height - 10;
            ContextualMenu.Layer.ShadowPath = CGPath.FromRect(ContextualMenu.Layer.Bounds);
            View.LayoutIfNeeded();
        }

        private void replaceContextualMenuActions(IImmutableList<CalendarMenuAction> actions)
        {
            if (actions == null || actions.Count == 0) return;

            ContextualMenuStackView.ArrangedSubviews.ForEach(view => view.RemoveFromSuperview());

            actions.Select(action => new CalendarContextualMenuActionView(action)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                })
                .Do(ContextualMenuStackView.AddArrangedSubview);
        }

        private void showContextualMenu()
        {
            if (!contextualMenuInitialised) return;

            contextualMenuVisible?.Accept(true);
            View.LayoutIfNeeded();
            ContextualMenuBottonConstraint.Constant = 0;
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.EaseOut,
                () => View.LayoutIfNeeded(),
                scrollUpIfEditingItemIsCoveredByContextualMenu);
        }

        private void dismissContextualMenu()
        {
            if (!contextualMenuInitialised) return;

            ContextualMenuBottonConstraint.Constant = -ContextualMenu.Frame.Height - 10;
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.EaseOut,
                () => View.LayoutIfNeeded(),
                () =>
                {
                    contextualMenuVisible?.Accept(false);
                    updateContentInset(true);
                });
        }


        private void scrollUpIfEditingItemIsCoveredByContextualMenu()
        {
            var editingItemFrame = dataSource.FrameOfEditingItem();

            if (editingItemFrame == null) return;
            var editingItemTop = editingItemFrame.Value.Top - CalendarCollectionView.ContentOffset.Y;

            var shouldScrollUp = ContextualMenu.Frame.Top <= editingItemTop + additionalContentOffsetWhenContextualMenuIsVisible;
            if (!shouldScrollUp) return;

            var scrollDelta = editingItemTop - ContextualMenu.Frame.Top - additionalContentOffsetWhenContextualMenuIsVisible;

            var newContentOffset = new CGPoint(
                CalendarCollectionView.ContentOffset.X,
                CalendarCollectionView.ContentOffset.Y - scrollDelta);
            CalendarCollectionView.SetContentOffset(newContentOffset, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            updateContentInset();
            layout.InvalidateCurrentTimeLayout();
        }

        private void updateContentInset(bool animate = false)
        {
            var topInset = collectionViewDefaultInset;
            var sideInset = 0;

            var bottomInset = contextualMenuVisible.Value
                ? collectionViewDefaultInset * 2 + ContextualMenu.Frame.Height
                : collectionViewDefaultInset * 2;

            bottomInset += runningTimeEntryCardHeight.Value;

            if (animate)
            {
                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.EaseOut,
                    () => CalendarCollectionView.ContentInset = new UIEdgeInsets(
                        topInset, sideInset, bottomInset, sideInset)
                    );
            }
            else
            {
                CalendarCollectionView.ContentInset = new UIEdgeInsets(
                    topInset, sideInset, bottomInset, sideInset);
            }
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            updateContentInset();
        }

        public void ScrollToTop()
        {
            CalendarCollectionView?.SetContentOffset(CGPoint.Empty, true);
        }

        public void SetGoodScrollPoint()
        {
            var frameHeight =
                CalendarCollectionView.Frame.Height
                - CalendarCollectionView.ContentInset.Top
                - CalendarCollectionView.ContentInset.Bottom;
            var hoursOnScreen = frameHeight / (CalendarCollectionView.ContentSize.Height / 24);
            var centeredHour = calculateCenteredHour(timeService.CurrentDateTime.ToLocalTime().TimeOfDay.TotalHours, hoursOnScreen);

            var offsetY = (centeredHour / 24) * CalendarCollectionView.ContentSize.Height - (frameHeight / 2);
            var scrollPointY = offsetY.Clamp(0, CalendarCollectionView.ContentSize.Height - frameHeight);
            var offset = new CGPoint(0, scrollPointY);
            CalendarCollectionView.SetContentOffset(offset, false);
        }

        private static double calculateCenteredHour(double currentHour, double hoursOnScreen)
        {
            var hoursPerHalfOfScreen = hoursOnScreen / 2;
            var minimumOffset = hoursOnScreen * minimumOffsetOfCurrentTimeIndicatorFromScreenEdge;

            var center = (currentHour + middleOfTheDay) / 2;

            if (currentHour < center - hoursPerHalfOfScreen + minimumOffset)
            {
                // the current time indicator would be too close to the top edge of the screen
                return currentHour - minimumOffset + hoursPerHalfOfScreen;
            }

            if (currentHour > center + hoursPerHalfOfScreen - minimumOffset)
            {
                // the current time indicator would be too close to the bottom edge of the screen
                return currentHour + minimumOffset - hoursPerHalfOfScreen;
            }

            return center;
        }
    }
}
