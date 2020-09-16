using System;
using System.Linq;
using Foundation;
using Toggl.Core.Analytics;
using CoreGraphics;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAnimation;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.iOS.ViewSources;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using UIKit;
using static Toggl.Core.UI.Helper.Animation;
using static Toggl.Core.Helper.Constants;

namespace Toggl.iOS.ViewControllers
{
    public sealed partial class CalendarViewController : ReactiveViewController<CalendarViewModel>, IUIPageViewControllerDataSource, IUIPageViewControllerDelegate
    {
        private const int weekViewHeight = 44;
        private const int maxAllowedPageIndex = CalendarMaxFutureDays - 1;
        private const int minAllowedPageIndex = -CalendarMaxPastDays + 1;
        private const int weekViewHeaderFontSize = 12;
        private const float showCardDelay = 0.1f;

        private readonly BehaviorRelay<bool> contextualMenuVisible = new BehaviorRelay<bool>(false);
        private readonly BehaviorRelay<nfloat> runningTimeEntryCardHeight = new BehaviorRelay<nfloat>(0);
        private readonly BehaviorRelay<string> timeTrackedOnDay = new BehaviorRelay<string>("");
        private readonly BehaviorRelay<int> currentPageRelay = new BehaviorRelay<int>(0);
        private readonly UIPageViewController pageViewController;
        private readonly UILabel[] weekViewHeaderLabels;
        private readonly UICollectionViewFlowLayout weekViewCollectionViewLayout;

        private CalendarWeeklyViewDayCollectionViewSource weekViewCollectionViewSource;
        private DateTime currentlyShownDate;
        private CancellationTokenSource cardAnimationCancellation;

        public CalendarViewController(CalendarViewModel calendarViewModel)
            : base(calendarViewModel, nameof(CalendarViewController))
        {
            pageViewController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal);
            weekViewHeaderLabels = Enumerable.Range(0, 7)
                .Select(_ => new UILabel
                {
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.SystemFontOfSize(weekViewHeaderFontSize, UIFontWeight.Medium),
                    TextColor = ColorAssets.CalendarHeaderLabel
                })
                .ToArray();

            weekViewCollectionViewLayout= new UICollectionViewFlowLayout
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                MinimumLineSpacing = 0
            };
        }

        public override void LoadView()
        {
            base.LoadView();
            setupWeekViewHeaderLabels();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;

            weekViewCollectionViewSource = new CalendarWeeklyViewDayCollectionViewSource(WeekViewCollectionView);
            currentlyShownDate = ViewModel.CurrentlyShownDate.Value;

            setupViews();

            ViewModel.WeekViewDays
                .Subscribe(weekViewCollectionViewSource.UpdateItems)
                .DisposedBy(DisposeBag);

            ViewModel.WeekViewHeaders
                .Subscribe(updateWeeklyViewHeaderLabelTexts)
                .DisposedBy(DisposeBag);

            weekViewCollectionViewSource.DaySelected
                .Subscribe(ViewModel.SelectDayFromWeekView.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.CurrentlyShownDate
                .Subscribe(weekViewCollectionViewSource.UpdateCurrentlySelectedDate)
                .DisposedBy(DisposeBag);

            ViewModel.CurrentlyShownDate
                .Subscribe(updateCurrentlyShownViewController)
                .DisposedBy(DisposeBag);

            ViewModel.CurrentlyShownDateString
                .Subscribe(SelectedDateLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            SettingsButton.Rx()
                .BindAction(ViewModel.OpenSettings)
                .DisposedBy(DisposeBag);

            contextualMenuVisible
                .Select(CommonFunctions.Invert)
                .Subscribe(setPageViewControllerEnabled)
                .DisposedBy(DisposeBag);

            contextualMenuVisible
                .Subscribe(contextualMenuVisible => WeekViewCollectionView.UserInteractionEnabled = !contextualMenuVisible)
                .DisposedBy(DisposeBag);

            contextualMenuVisible
                .Subscribe(toggleTabBar)
                .DisposedBy(DisposeBag);

            timeTrackedOnDay
                .Subscribe(DailyTrackedTimeLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            var trackModeImage = UIImage.FromBundle("play").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var manualModeImage = UIImage.FromBundle("plusLarge").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            ViewModel.IsInManualMode
                .Select(isInManualMode => isInManualMode ? manualModeImage : trackModeImage)
                .Subscribe(image => StartTimeEntryButtonIcon.Image = image)
                .DisposedBy(DisposeBag);

            CurrentTimeEntryCard.Rx().Tap()
                .WithLatestFrom(ViewModel.CurrentRunningTimeEntry, (_, te) => te)
                .Where(te => te != null)
                .Select(te => new EditTimeEntryInfo(EditTimeEntryOrigin.RunningTimeEntryCard, te.Id))
                .Subscribe(ViewModel.SelectTimeEntry.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.CurrentRunningTimeEntry
                .Select(te => te?.Description)
                .Subscribe(CurrentTimeEntryDescriptionLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            ViewModel.ElapsedTime
                .Subscribe(CurrentTimeEntryElapsedTimeLabel.Rx().Text())
                .DisposedBy(DisposeBag);

            var capHeight = CurrentTimeEntryProjectTaskClientLabel.Font.CapHeight;
            var clientColor = ColorAssets.Text3;
            ViewModel.CurrentRunningTimeEntry
                .Select(te => te?.ToFormattedTimeEntryString(capHeight, clientColor, shouldColorProject: true))
                .Subscribe(CurrentTimeEntryProjectTaskClientLabel.Rx().AttributedText())
                .DisposedBy(DisposeBag);


            CurrentTimeEntryCard.IsAccessibilityElementFocused
                .CombineLatest(ViewModel.CurrentRunningTimeEntry,
                    (_, runningEntry) => createAccessibilityLabelForRunningEntryCard(runningEntry))
                .Subscribe(CurrentTimeEntryCard.Rx().AccessibilityLabel())
                .DisposedBy(DisposeBag);

            ViewModel.IsTimeEntryRunning
                .CombineLatest(contextualMenuVisible, shouldShowTimeEntryCard)
                .Where(CommonFunctions.Identity)
                .Subscribe(_ => showTimeEntryCard())
                .DisposedBy(DisposeBag);

            ViewModel.IsTimeEntryRunning
                .CombineLatest(contextualMenuVisible, shouldShowStartButton)
                .Where(CommonFunctions.Identity)
                .Subscribe(_ => showStartButton())
                .DisposedBy(DisposeBag);

            contextualMenuVisible
                .Where(CommonFunctions.Identity)
                .Subscribe(_ => hideTimeEntryCardAndStartButton())
                .DisposedBy(DisposeBag);

            StartTimeEntryButton.Rx()
                .BindAction(ViewModel.StartTimeEntry, _ => true, ButtonEventType.TapGesture)
                .DisposedBy(DisposeBag);

            StartTimeEntryButton.Rx()
                .BindAction(ViewModel.StartTimeEntry, _ => false, ButtonEventType.LongPress, useFeedback: true)
                .DisposedBy(DisposeBag);

            StopTimeEntryButton.Rx()
                .BindAction(ViewModel.StopTimeEntry)
                .DisposedBy(DisposeBag);
        }

        private void toggleTabBar(bool hidden)
        {
            TabBarController.TabBar.Hidden = hidden;
        }

        private void setupViews()
        {
            DailyTrackedTimeLabel.Font = DailyTrackedTimeLabel.Font.GetMonospacedDigitFont();

            pageViewController.DataSource = this;
            pageViewController.Delegate = this;
            pageViewController.View.Frame = DayViewContainer.Bounds;
            DayViewContainer.AddSubview(pageViewController.View);
            pageViewController.DidMoveToParentViewController(this);

            var viewControllers = new[] { viewControllerAtIndex(0) };
            viewControllers[0].SetGoodScrollPoint();
            pageViewController.SetViewControllers(viewControllers, UIPageViewControllerNavigationDirection.Forward, false, null);

            WeekViewCollectionView.Source = weekViewCollectionViewSource;
            WeekViewCollectionView.ShowsHorizontalScrollIndicator = false;
            WeekViewCollectionView.CollectionViewLayout = weekViewCollectionViewLayout;
            WeekViewCollectionView.DecelerationRate = UIScrollView.DecelerationRateFast;

            StartTimeEntryButton.AccessibilityLabel = Resources.StartTimeEntry;
            StopTimeEntryButton.AccessibilityLabel = Resources.StopCurrentlyRunningTimeEntry;

            prepareStartButtonLongPressAnimation();
            setupRunningTimeEntryCard();
        }

        private void setPageViewControllerEnabled(bool enabled)
        {
            pageViewController.DataSource = enabled ? this : null;
            pageViewController.Delegate = enabled ? this : null;
        }

        private void updateCurrentlyShownViewController(DateTime newDate)
        {
            if (newDate == currentlyShownDate)
                return;

            var direction = newDate > currentlyShownDate
                ? UIPageViewControllerNavigationDirection.Forward
                : UIPageViewControllerNavigationDirection.Reverse;
            currentlyShownDate = newDate;

            var today = IosDependencyContainer.Instance.TimeService.CurrentDateTime.ToLocalTime().Date;
            var index = (newDate - today).Days;

            var currentViewController = pageViewController.ViewControllers[0] as CalendarDayViewController;
            var newViewController = viewControllerAtIndex(index);
            if (currentViewController != null)
                newViewController.SetScrollOffset(currentViewController.ScrollOffset);
            pageViewController.SetViewControllers(new[] {newViewController},  direction, true, null);
            currentPageRelay.Accept(index);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            updateWeekViewHeaderWidthConstraints();
            weekViewCollectionViewSource.UpdateCurrentlySelectedDate(ViewModel.CurrentlyShownDate.Value);
            ViewModel.RealoadWeekView();
        }

        public UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var referenceTag = referenceViewController.View.Tag;
            if (referenceTag == minAllowedPageIndex)
                return null;

            return viewControllerAtIndex(referenceTag - 1);
        }

        public UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var referenceTag = referenceViewController.View.Tag;
            if (referenceTag == maxAllowedPageIndex)
                return null;

            return viewControllerAtIndex(referenceTag + 1);
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            pageViewController.ViewControllers.ForEach(viewController => viewController.ViewWillTransitionToSize(toSize, coordinator));
        }

        [Export("pageViewController:willTransitionToViewControllers:")]
        public void WillTransition(UIPageViewController pageViewController, UIViewController[] pendingViewControllers)
        {
            var pendingCalendarDayViewController = pendingViewControllers.FirstOrDefault() as CalendarDayViewController;
            if (pendingCalendarDayViewController == null)
                return;

            var currentCalendarDayViewController = pageViewController.ViewControllers.FirstOrDefault() as CalendarDayViewController;
            if (currentCalendarDayViewController == null) return;

            pendingCalendarDayViewController.SetScrollOffset(currentCalendarDayViewController.ScrollOffset);
        }

        private CalendarDayViewController viewControllerAtIndex(nint index)
        {
            var viewModel = ViewModel.DayViewModelAt((int) index);
            var viewController = new CalendarDayViewController(viewModel, currentPageRelay, timeTrackedOnDay, contextualMenuVisible, runningTimeEntryCardHeight);
            viewController.View.Tag = index;
            return viewController;
        }

        [Export("pageViewController:didFinishAnimating:previousViewControllers:transitionCompleted:")]
        public void DidFinishAnimating(UIPageViewController pageViewController, bool finished, UIViewController[] previousViewControllers, bool completed)
        {
            if (!completed) return;

            var newIndex = pageViewController.ViewControllers.FirstOrDefault()?.View?.Tag;
            if (newIndex == null) return;

            var newDate = ViewModel.IndexToDate((int)newIndex.Value);

            currentlyShownDate = newDate;
            currentPageRelay.Accept((int)newIndex);
            ViewModel.CurrentlyShownDate.Accept(newDate);

            var previousIndex = previousViewControllers.FirstOrDefault()?.View?.Tag;
            if (previousIndex == null) return;
            var swipeDirection = previousIndex > newIndex
                ? CalendarSwipeDirection.Left
                : CalendarSwipeDirection.Rignt;
            var daysSinceToday = (int)newIndex.Value;
            var dayOfWeek = ViewModel.IndexToDate((int)newIndex.Value).DayOfWeek.ToString();
            IosDependencyContainer.Instance.AnalyticsService.CalendarSingleSwipe.Track(swipeDirection, daysSinceToday, dayOfWeek);
        }

        private void setupWeekViewHeaderLabels()
        {
            foreach (var dayHeader in weekViewHeaderLabels)
            {
                WeekViewDayHeaderContainer.AddSubview(dayHeader);
                dayHeader.TopAnchor.ConstraintEqualTo(WeekViewDayHeaderContainer.TopAnchor).Active = true;
                dayHeader.BottomAnchor.ConstraintEqualTo(WeekViewDayHeaderContainer.BottomAnchor).Active = true;
            }

            weekViewHeaderLabels.First().LeadingAnchor.ConstraintEqualTo(WeekViewDayHeaderContainer.LeadingAnchor).Active = true;
            weekViewHeaderLabels.Last().TrailingAnchor.ConstraintEqualTo(WeekViewDayHeaderContainer.TrailingAnchor).Active = true;

            for (int i = 1; i < 7; i++)
            {
                var previousDayHeader = weekViewHeaderLabels[i - 1];
                var currentDayHeader = weekViewHeaderLabels[i];
                previousDayHeader.TrailingAnchor.ConstraintEqualTo(currentDayHeader.LeadingAnchor).Active = true;
            }
        }

        private void setupRunningTimeEntryCard()
        {
            //Card border
            CurrentTimeEntryCard.Opaque = false;
            CurrentTimeEntryCard.Layer.CornerRadius = 8;
            CurrentTimeEntryCard.Layer.MaskedCorners = (CACornerMask)3;
            CurrentTimeEntryCard.Layer.ShadowColor = UIColor.Black.CGColor;
            CurrentTimeEntryCard.Layer.ShadowOffset = new CGSize(0, -2);
            CurrentTimeEntryCard.Layer.ShadowOpacity = 0.1f;
            CurrentTimeEntryElapsedTimeLabel.Font = CurrentTimeEntryElapsedTimeLabel.Font.GetMonospacedDigitFont();

            // Card animations
            StopTimeEntryButton.Hidden = true;
            CurrentTimeEntryCard.Hidden = true;

            //Hide play button for later animating it
            StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);

            // Open edit view for the currently running time entry by swiping up
            // Open edit view for the currently running time entry by swiping up
            var swipeUpRunningCardGesture = new UISwipeGestureRecognizer(async () =>
            {
                var currentlyRunningTimeEntry = await ViewModel.CurrentRunningTimeEntry.FirstAsync();
                if (currentlyRunningTimeEntry == null)
                    return;

                var selectTimeEntryData = new EditTimeEntryInfo(EditTimeEntryOrigin.RunningTimeEntryCard, currentlyRunningTimeEntry.Id);
                await ViewModel.SelectTimeEntry.ExecuteWithCompletion(selectTimeEntryData);
            });
            swipeUpRunningCardGesture.Direction = UISwipeGestureRecognizerDirection.Up;
            CurrentTimeEntryCard.AddGestureRecognizer(swipeUpRunningCardGesture);
        }

        private void updateWeekViewHeaderWidthConstraints()
        {
            var targetWidth = WeekViewContainer.Frame.Width / 7;
            weekViewCollectionViewLayout.ItemSize = new CGSize(targetWidth, weekViewHeight);
            foreach (var label in weekViewHeaderLabels)
            {
                var widthConstraint = label.Constraints.FirstOrDefault(constraint => constraint.FirstAttribute == NSLayoutAttribute.Width);
                if (widthConstraint == null)
                {
                    label.WidthAnchor.ConstraintEqualTo(targetWidth).Active = true;
                    continue;
                }

                widthConstraint.Constant = targetWidth;
                label.SetNeedsLayout();
            }
        }

        private void updateWeeklyViewHeaderLabelTexts(IImmutableList<DayOfWeek> headers)
        {
            if (headers.Count != 7)
                throw new ArgumentException($"The count {nameof(headers)} must be 7 (it was {headers.Count})");

            for (int i = 0; i < 7; i++)
                weekViewHeaderLabels[i].Text = textForDayHeader(headers[i]);
        }

        private string textForDayHeader(DayOfWeek dayOfWeek)
            => TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular
                ? dayOfWeek.FullName()
                : dayOfWeek.Initial();

        private string createAccessibilityLabelForRunningEntryCard(IThreadSafeTimeEntry timeEntry)
        {
            if (timeEntry == null)
                return null;

            var accessibilityLabel = Resources.CurrentlyRunningTimeEntry;

            var duration = IosDependencyContainer.Instance.TimeService.CurrentDateTime - timeEntry.Start;
            accessibilityLabel += $", {duration}";

            if (!string.IsNullOrEmpty(timeEntry.Description))
                accessibilityLabel += $", {timeEntry.Description}";

            var projectName = timeEntry.Project?.Name ?? "";
            if (!string.IsNullOrEmpty(projectName))
                accessibilityLabel += $", {Resources.Project}: {projectName}";

            var taskName = timeEntry.Task?.Name ?? "";
            if (!string.IsNullOrEmpty(taskName))
                accessibilityLabel += $", {Resources.Task}: {taskName}";

            var clientName = timeEntry.Project?.Client?.Name ?? "";
            if (!string.IsNullOrEmpty(clientName))
                accessibilityLabel += $", {Resources.Client}: {clientName}";

            return accessibilityLabel;
        }

        private bool shouldShowTimeEntryCard(bool isTimeEntryRunning, bool isContextualMenuVisible)
            => !isContextualMenuVisible && isTimeEntryRunning;

        private void showTimeEntryCard()
        {
            runningTimeEntryCardHeight.Accept(CurrentTimeEntryCard.Frame.Height);
            StopTimeEntryButton.Hidden = false;
            CurrentTimeEntryCard.Hidden = false;

            cardAnimationCancellation?.Cancel();
            cardAnimationCancellation = new CancellationTokenSource();

            AnimationExtensions.Animate(Timings.EnterTiming, showCardDelay, Curves.EaseOut,
                () => StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f),
                () =>
                {
                    AnimationExtensions.Animate(Timings.LeaveTimingFaster, Curves.EaseIn,
                        () => StopTimeEntryButton.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f),
                        cancellationToken: cardAnimationCancellation.Token);

                    AnimationExtensions.Animate(Timings.LeaveTiming, Curves.CardOutCurve,
                        () =>
                        {
                            CurrentTimeEntryCard.Transform = CGAffineTransform.MakeTranslation(0, 0);
                            CurrentTimeEntryCard.Alpha = 1;
                        },
                        cancellationToken: cardAnimationCancellation.Token);
                },
                cancellationToken: cardAnimationCancellation.Token);
        }

        private bool shouldShowStartButton(bool isTimeEntryRunning, bool isContextualMenuVisible)
            => !isContextualMenuVisible && !isTimeEntryRunning;

        private void showStartButton()
        {
            runningTimeEntryCardHeight.Accept(0);
            cardAnimationCancellation?.Cancel();
            cardAnimationCancellation = new CancellationTokenSource();

            AnimationExtensions.Animate(Timings.LeaveTimingFaster, Curves.EaseIn,
                () => StopTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f),
                () => StopTimeEntryButton.Hidden = true,
                cancellationToken: cardAnimationCancellation.Token);

            AnimationExtensions.Animate(Timings.LeaveTiming, Curves.CardOutCurve,
                () =>
                {
                    CurrentTimeEntryCard.Transform =
                        CGAffineTransform.MakeTranslation(0, CurrentTimeEntryCard.Frame.Height);
                    CurrentTimeEntryCard.Alpha = 0;
                },
                () =>
                {
                    CurrentTimeEntryCard.Hidden = true;

                    AnimationExtensions.Animate(Timings.EnterTiming, Curves.EaseOut,
                        () => StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(1f, 1f),
                        cancellationToken: cardAnimationCancellation.Token);
                },
                cancellationToken: cardAnimationCancellation.Token);
        }

        private void hideTimeEntryCardAndStartButton()
        {
            runningTimeEntryCardHeight.Accept(0);
            StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
            StopTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);
            CurrentTimeEntryCard.Transform = CGAffineTransform.MakeTranslation(0, CurrentTimeEntryCard.Frame.Height);
            CurrentTimeEntryCard.Alpha = 0;
        }

        private void prepareStartButtonLongPressAnimation()
        {
            const double longPressMinimumPressDuration = 0.5; // default OS long press duration is 0.5s
            const double startAnimatingAfter = 0.1;
            const double bounceAnimationDuration = 0.1f;
            const double shrinkingAnimationDuration = longPressMinimumPressDuration - startAnimatingAfter - bounceAnimationDuration;
            nfloat noDelay = 0.0f;

            var shrunk = CGAffineTransform.MakeScale(0.9f, 0.9f);
            var bigger = CGAffineTransform.MakeScale(1.05f, 1.05f);
            var normalScale = CGAffineTransform.MakeScale(1f, 1f);

            var cts = new CancellationTokenSource();
            var press = new UILongPressGestureRecognizer(startButtonAnimation);
            press.MinimumPressDuration = startAnimatingAfter;
            press.ShouldRecognizeSimultaneously = (_, __) => true;

            StartTimeEntryButton.AddGestureRecognizer(press);

            void startButtonAnimation(UIGestureRecognizer recognizer)
            {
                switch (recognizer.State)
                {
                    case UIGestureRecognizerState.Began:
                        startShrinkingAnimation();
                        break;

                    case UIGestureRecognizerState.Cancelled:
                    case UIGestureRecognizerState.Failed:
                        cts?.Cancel();
                        cts = new CancellationTokenSource();
                        backToNormal();
                        break;
                }
            }

            void startShrinkingAnimation()
            {
                AnimationExtensions.Animate(
                    shrinkingAnimationDuration,
                    noDelay,
                    Animation.Curves.Bounce,
                    () => StartTimeEntryButton.Transform = shrunk,
                    expand,
                    cts.Token);
            }

            void expand()
            {
                AnimationExtensions.Animate(
                    bounceAnimationDuration / 2,
                    noDelay,
                    Animation.Curves.Bounce,
                    () => StartTimeEntryButton.Transform = bigger,
                    backToNormal,
                    cts.Token);
            }

            void backToNormal()
            {
                AnimationExtensions.Animate(
                    bounceAnimationDuration / 2,
                    noDelay,
                    Animation.Curves.Bounce,
                    () => StartTimeEntryButton.Transform = normalScale,
                    cancellationToken: cts.Token);
            }
        }
    }
}
