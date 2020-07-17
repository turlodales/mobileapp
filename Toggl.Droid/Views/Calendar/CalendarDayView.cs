using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Toggl.Core;
using Toggl.Core.Calendar;
using Toggl.Core.Extensions;
using Toggl.Core.UI.Calendar;
using Toggl.Droid.Extensions;
using Toggl.Droid.ViewHelpers;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Constants = Toggl.Core.Helper.Constants;
using Math = System.Math;

namespace Toggl.Droid.Views.Calendar
{
    [Register("toggl.droid.views.CalendarDayView")]
    public partial class CalendarDayView : View
    {
        private const int hoursPerDay = Constants.HoursPerDay;
        private const int vibrationDurationInMilliseconds = 5;
        private const int vibrationAmplitude = 7;
        private const int scrollAnimationDurationInMillis = 300;
        
        private readonly ISubject<CalendarItem?> calendarItemTappedSubject = new Subject<CalendarItem?>();
        private readonly ISubject<DateTimeOffset> emptySpansTouchedObservable = new Subject<DateTimeOffset>();
        private readonly BehaviorSubject<int> scrollOffsetSubject = new BehaviorSubject<int>(0);
        private readonly RectF tapCheckRectF = new RectF();
        private readonly TimeSpan defaultDuration = TimeSpan.FromMinutes(30);
        private readonly RectF viewFrame = new RectF();
        
        private BehaviorSubject<int> hourHeightSubject;
        private GestureDetector gestureDetector;
        private ScaleGestureDetector scaleGestureDetector;
        private OverScroller scroller;
        private Handler handler;
        private ITimeService timeService;
        private CalendarLayoutCalculator calendarLayoutCalculator;
        private DateTime currentDate = DateTime.Now;
        private Paint currentHourPaint;
        private Vibrator hapticFeedbackProvider;
        private ImmutableList<DateTimeOffset> allItemsStartAndEndTime = ImmutableList<DateTimeOffset>.Empty;
        private bool shouldDrawCurrentHourIndicator;
        private bool isScrolling;
        private bool flingWasCalled;
        private float availableWidth;
        private float distanceFromEdgesToTriggerAutoScroll;
        private float topAreaTriggerLine;
        private float bottomAreaTriggerLine;
        private int currentHourCircleRadius;
        private int autoScrollToFrameExtraDistance;
        private int baseHourHeight;
        private int maxHourHeight;

        private int hourHeight => hourHeightSubject.Value;
        private int scrollOffset => scrollOffsetSubject.Value;
        private int maxHeight => hourHeight * hoursPerDay;

        private ImmutableList<CalendarItem> originalCalendarItems = ImmutableList<CalendarItem>.Empty;

        private ImmutableList<CalendarItem> calendarItems = ImmutableList<CalendarItem>.Empty;
        private ImmutableList<CalendarItemRectAttributes> calendarItemLayoutAttributes = ImmutableList<CalendarItemRectAttributes>.Empty;

        public IObservable<CalendarItem?> CalendarItemTappedObservable
            => calendarItemTappedSubject.AsObservable();

        public IObservable<DateTimeOffset> EmptySpansTouchedObservable
            => emptySpansTouchedObservable.AsObservable();

        public IObservable<int> ScrollOffsetObservable
            => scrollOffsetSubject.AsObservable();

        public IObservable<int> HourHeight
            => hourHeightSubject.AsObservable();

        #region Constructors

        protected CalendarDayView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CalendarDayView(Context context) : base(context)
        {
            init();
        }

        public CalendarDayView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init();
        }

        public CalendarDayView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            init();
        }

        public CalendarDayView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            init();
        }

        #endregion

        private void init()
        {
            baseHourHeight = Context.GetDimen(Resource.Dimension.calendarBaseHourHeight);
            hourHeightSubject = new BehaviorSubject<int>(baseHourHeight);
            maxHourHeight = Context.GetDimen(Resource.Dimension.calendarMaxHourHeight);
            distanceFromEdgesToTriggerAutoScroll = Context.GetDimen(Resource.Dimension.calendarEdgeDistanceToTriggerAutoScroll);
            autoScrollToFrameExtraDistance = Context.GetDimen(Resource.Dimension.calendarEventAutoScrollToFrameExtraDistance);
            currentHourCircleRadius = Context.GetDimen(Resource.Dimension.calendarCurrentHourCircleRadius);
            timeService = AndroidDependencyContainer.Instance.TimeService;
            calendarLayoutCalculator = new CalendarLayoutCalculator(timeService);
            hapticFeedbackProvider = (Vibrator)Context.GetSystemService(Context.VibratorService);
            var calendarGestureListener = new CalendarGestureListener(
                onTouchDown,
                onLongPress,
                scrollView,
                flingView,
                onSingleTapUp,
                onScale);

            gestureDetector = new GestureDetector(Context, calendarGestureListener);
            scaleGestureDetector = new ScaleGestureDetector(Context, calendarGestureListener);
            scroller = new OverScroller(Context);
            handler = new Handler(Looper.MainLooper);
            currentHourPaint = new Paint(PaintFlags.AntiAlias);
            currentHourPaint.Color = Context.SafeGetColor(Resource.Color.currentHourColor);
            currentHourPaint.StrokeWidth = Context.GetDimen(Resource.Dimension.calendarCurrentHourIndicatorStrokeSize);
            currentHourPaint.SetStyle(Paint.Style.FillAndStroke);

            initBackgroundBackingFields();
            initEventDrawingBackingFields();
            initEventEditionBackingFields();
        }

        public void DiscardEditModeChanges()
        {
            updateItemsAndRecalculateEventsAttrs(originalCalendarItems);
        }
        
        public void ClearEditMode()
        {
            editAction = EditAction.None;
            itemEditInEditMode = CalendarItemEditInfo.None;
            Invalidate();
        }
        
        public void SetCurrentDate(DateTimeOffset dateOnView)
        {
            currentDate = dateOnView.LocalDateTime.Date;
            var today = timeService.CurrentDateTime.LocalDateTime.Date;
            shouldDrawCurrentHourIndicator = currentDate == today;
        }

        public void UpdateCalendarHoursFormat(TimeFormat timeFormat)
        {
            timeOfDayFormat = timeFormat;
            var newHours = createHours();
            hours = newHours;
            Invalidate();
        }

        public void SetCurrentItemInEditMode(CalendarItem? calendarItemInEditMode)
        {
            var isCurrentlyEditingItem = isEditingItem();
            if (!calendarItemInEditMode.HasValue)
            {
                if (isCurrentlyEditingItem)
                {
                    cancelCurrentEdition();
                }
                else
                {
                    ClearEditMode();
                }
                return;
            }

            var calendarItem = calendarItemInEditMode.Value;
            if (!isCurrentlyEditingItem && calendarItem.Source == CalendarItemSource.Calendar)
                return;
            
            if (isCurrentlyEditingItem)
            {
                if (calendarItem.Id == itemEditInEditMode.CalendarItem.Id)
                    return;
                
                cancelCurrentEdition();
                if (calendarItem.Source == CalendarItemSource.Calendar)
                    return;
            }

            beginEdition(calendarItem);
        }
        
        private void beginEdition(CalendarItem calendarItem)
        {
            var calendarItemsToSearch = calendarItems;
            if (string.IsNullOrEmpty(calendarItem.Id))
            {
                var indexToInsertNewItem = calendarItemsToSearch.FindIndex(item => item.StartTime >= calendarItem.StartTime);
                indexToInsertNewItem = indexToInsertNewItem < 0 ? calendarItemsToSearch.Count : indexToInsertNewItem;
                updateItemsAndRecalculateEventsAttrs(calendarItemsToSearch.Insert(indexToInsertNewItem, calendarItem));
            }

            calendarItemsToSearch = calendarItems;
            var calendarLayoutItems = calendarItemLayoutAttributes;
            var calendarItemIndex = calendarItemsToSearch.IndexOf(calendarItem);
            if (calendarItemIndex < 0)
            {
                cancelCurrentEdition();
                return;
            }
            
            var itemInEditMode = new CalendarItemEditInfo(calendarItem, calendarLayoutItems[calendarItemIndex], calendarItemIndex, hourHeight, minHourHeight, timeService.CurrentDateTime);
            
            itemEditInEditMode = itemInEditMode;
            itemInEditMode.CalculateRect(itemInEditModeRect);
            updateEditingStartEndLabels();
            allItemsStartAndEndTime = selectItemsStartAndEndTime();
            Invalidate();
        }

        partial void initBackgroundBackingFields();
        partial void initEventDrawingBackingFields();
        partial void initEventEditionBackingFields();

        public override bool CanScrollHorizontally(int direction)
            => false;
        
        public override bool CanScrollVertically(int direction)
        {
            if (direction < 0)
                return scrollOffset > 0;
            return scrollOffset < maxHeight - Height;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            viewFrame.Set(0f, scrollOffset, Width, scrollOffset + Height);
            topAreaTriggerLine = viewFrame.Top + distanceFromEdgesToTriggerAutoScroll;
            bottomAreaTriggerLine = viewFrame.Bottom - distanceFromEdgesToTriggerAutoScroll;
            
            canvas.Save();
            canvas.Translate(0f, -scrollOffset);

            canvas.ClipRect(0, scrollOffset, Width, Height + scrollOffset);
            drawHourLines(canvas);
            drawCalendarItems(canvas);
            drawCurrentHourIndicator(canvas);

            canvas.Restore();
        }

        public void ScrollToCurrentHour(bool scrollSmoothly = false)
        {
            var hourOffset = calculateCurrentHourOffset();
            isScrolling = true;
            if (scrollSmoothly)
                scroller.StartScroll(0, scrollOffset, 0, hourOffset - scrollOffset, scrollAnimationDurationInMillis);
            else
                scroller.StartScroll(0, scrollOffset, 0, hourOffset - scrollOffset);
            
            continueScroll();
        }

        private int calculateCurrentHourOffset()
        {
            var now = timeService.CurrentDateTime.LocalDateTime;
            return (int)calculateHourOffsetFrom(now, hourHeight);
        }

        private void drawCurrentHourIndicator(Canvas canvas)
        {
            if (!shouldDrawCurrentHourIndicator) return;
            
            var currentHourY = calculateCurrentHourOffset();
            canvas.DrawLine(timeSliceStartX, currentHourY, Width, currentHourY, currentHourPaint);
            canvas.DrawCircle(timeSliceStartX, currentHourY, currentHourCircleRadius, currentHourPaint);
        }
        
        partial void drawHourLines(Canvas canvas);
        partial void drawCalendarItems(Canvas canvas);

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            availableWidth = Width - leftMargin;
            if (scrollOffsetIsPastBottomFrame())
            {
                scrollOffsetSubject.OnNext(maxHeight - Height);
            }
            availableWidth = Width - leftMargin;

            if (!changed)
                return;

            processBackgroundOnLayout();
            processEventsOnLayout();
        }

        public void SetOffset(int offset)
        {
            if (offset == scrollOffset)
                return;
            
            scrollOffsetSubject.OnNext(offset);
            Invalidate();
        }

        public void SetHourHeight(int hourHeight)
        {
            if (hourHeight == this.hourHeight)
                return;

            hourHeightSubject.OnNext(hourHeight);
            processBackgroundOnLayout();
            processEventsOnLayout();
            Invalidate();
        }

        partial void processBackgroundOnLayout();
        partial void processEventsOnLayout();

        private void continueScroll()
        {
            isScrolling = isScrolling && scroller.ComputeScrollOffset();
            if (!isScrolling)
            {
                Invalidate();
                return;
            }

            if (scroller.CurrY == scrollOffset)
            {
                handler.Post(continueScroll);
                return;
            }

            var oldScrollOffset = scrollOffset;
            scrollOffsetSubject.OnNext(
                scroller.CurrY.Clamp(0, maxHeight - Height));

            OnScrollChanged(0, scrollOffset, 0, oldScrollOffset);

            handler.Post(continueScroll);
            Invalidate();
        }

        private bool scrollOffsetIsPastBottomFrame()
            => scrollOffset >= maxHeight - Height;

        private void onTouchDown(MotionEvent e1)
        {
            scroller.ForceFinished(true);
            flingWasCalled = false;
            handler.RemoveCallbacks(continueScroll);

            onTouchDownWhileEditingItem(e1);
            
            Invalidate();
        }
        
        private void onLongPress(MotionEvent e1)
        {
            var touchX = e1.GetX();
            var touchY = e1.GetY();
            var calendarItemInfo = findCalendarItemFromPoint(touchX, touchY);
            
            if (calendarItemInfo.IsValid) 
                return;

            var startTime = dateAtYOffset(touchY + scrollOffset);
            var duration = defaultDuration;
            var teGaps = calendarLayoutCalculator.CalculateTwoHoursOrLessGapsLayoutAttributes(calendarItems);
            var matchingGap = teGaps.FirstOrDefault(gap => startTime > gap.StartTime && startTime < gap.StartTime + gap.Duration);
            duration = matchingGap.Duration == default ? duration : matchingGap.Duration;
            startTime = matchingGap.StartTime == default ? startTime : matchingGap.StartTime;
            
            var newCalendarItem = new CalendarItem(string.Empty, string.Empty, CalendarItemSource.TimeEntry, startTime, duration, Shared.Resources.NewTimeEntry, CalendarIconKind.None);
            calendarItemTappedSubject.OnNext(newCalendarItem);
        }

        private bool onScale(ScaleGestureDetector detector)
        {
            var oldHourHeight = hourHeight;
            var scaleFactor = detector.ScaleFactor;
            var newHourHeight =(int)(hourHeight * scaleFactor);
            hourHeightSubject.OnNext(
                newHourHeight.Clamp(baseHourHeight, maxHourHeight));

            var hourSizeChanged = oldHourHeight != hourHeight;
            if (!hourSizeChanged)
                return true;

            // Since the size of each hour is an integer
            // we first need to calculate the real scale
            // factor applied to the calendar
            var actualScale = (newHourHeight - oldHourHeight) / (((float)newHourHeight + oldHourHeight) / 2);

            // We need to calculate so the calendar feels
            // like it's zooming in and not sliding below
            // the user's fingers
            var focusPointOffset = detector.FocusY * actualScale;
            var scaledOffset = scrollOffset * actualScale;
            var newScrollOffset = scrollOffset + scaledOffset + focusPointOffset;
            scrollOffsetSubject.OnNext(
                (int)newScrollOffset.Clamp(0, maxHeight - Height));
            
            processBackgroundOnLayout();
            processEventsOnLayout();
            Invalidate();

            return true;
        }

        private ImmutableList<DateTimeOffset> selectItemsStartAndEndTime()
        {
            var calendarItemsToSelect = calendarItems;
            var startTimes = calendarItemsToSelect.Select(item => item.StartTime).Distinct();
            var endTimes = calendarItemsToSelect.Where(item => item.EndTime.HasValue).Select(item => (DateTimeOffset)item.EndTime).Distinct();
            return startTimes.Concat(endTimes).ToImmutableList();
        }

        private void onSingleTapUp(MotionEvent e1)
        {
            var touchX = e1.GetX();
            var touchY = e1.GetY();

            var calendarItemInfo = findCalendarItemFromPoint(touchX, touchY);
            var touchedEmptySpace = !calendarItemInfo.IsValid;

            if (touchedEmptySpace)
            {
                calendarItemTappedSubject.OnNext(null);
                return;
            }
            
            calendarItemTappedSubject.OnNext(calendarItemInfo.CalendarItem);
        }

        private void cancelCurrentEdition()
        {
            ClearEditMode();
            DiscardEditModeChanges();
        }
        
        private bool isEditingItem() => itemEditInEditMode.IsValid;

        private CalendarItemEditInfo findCalendarItemFromPoint(float x, float y)
        {
            var currentItemInEditMode = itemEditInEditMode;
            var calendarItemsAvailableDuringSearch = calendarItems;
            var itemsToSearch = calendarItemLayoutAttributes;

            if (currentItemInEditMode.IsValid)
            {
                currentItemInEditMode.CalculateRect(tapCheckRectF);
                if (tapCheckRectF.Contains(x, y + scrollOffset)) 
                    return currentItemInEditMode;
            }
            
            for (var i = 0; i < itemsToSearch.Count; i++)
            {
                if (currentItemInEditMode.IsValid && currentItemInEditMode.OriginalIndex == i) 
                    continue;
                
                var calendarItemAttr = itemsToSearch[i];
                calendarItemAttr.CalculateRect(hourHeight, minHourHeight, tapCheckRectF);
                if (tapCheckRectF.Contains(x, y + scrollOffset))
                    return new CalendarItemEditInfo(calendarItemsAvailableDuringSearch[i], itemsToSearch[i], i, hourHeight, minHourHeight, timeService.CurrentDateTime);
            }

            return CalendarItemEditInfo.None;
        }

        private void scrollView(MotionEvent e1, MotionEvent e2, float deltaX, float deltaY)
        {
            if (isDragging) 
                return;

            var oldScrollOffset = scrollOffset;
            var newScrollOffset = scrollOffset + (int)deltaY;
            scrollOffsetSubject.OnNext(
                newScrollOffset.Clamp(0, maxHeight - Height));

            OnScrollChanged(0, scrollOffset, 0, oldScrollOffset);

            isScrolling = true;
            PostInvalidate();
        }

        private void flingView(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (editAction != EditAction.None) return;
            
            scroller.ForceFinished(true);

            flingWasCalled = true;
            isScrolling = true;
            scroller.Fling(0, scrollOffset, 0, (int) (-velocityY / 2f), 0, 0, 0, maxHeight);

            handler.Post(continueScroll);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var scaleResult = scaleGestureDetector.OnTouchEvent(e);
            if (scaleGestureDetector.IsInProgress)
                return scaleResult || base.OnTouchEvent(e);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    shouldTryToAutoScrollToEvent = false;
                    gestureDetector.OnTouchEvent(e);
                    return true;

                case MotionEventActions.Up:
                    isDragging = false;
                    cancelDraggingAndAutoScroll();
                    gestureDetector.OnTouchEvent(e);
                    updateLayoutIfNeededDuringEdition();
                    if (scrollFrameToDisplayItemInEditModeIfNeeded())
                        return true;
                    if (flingWasCalled)
                        return true;
                    if (!isScrolling)
                        return true;

                    isScrolling = false;
                    Invalidate();
                    return true;

                case MotionEventActions.Move:
                    gestureDetector.OnTouchEvent(e);
                    if (isDragging)
                    {
                        dragEvent(e);
                    }
                    return true;

                case MotionEventActions.Cancel:
                    isDragging = false;
                    cancelDraggingAndAutoScroll();
                    gestureDetector.OnTouchEvent(e);
                    isScrolling = false;
                    return true;

                default:
                    return gestureDetector.OnTouchEvent(e) || base.OnTouchEvent(e);
            }
        }

        private bool scrollFrameToDisplayItemInEditModeIfNeeded()
        {
            var currentItemInEditMode = itemEditInEditMode;
            if (!currentItemInEditMode.IsValid || !shouldTryToAutoScrollToEvent) return false;
            
            var startTime = currentItemInEditMode.CalendarItem.StartTime.ToLocalTime();
            var durationInPx = currentItemInEditMode.CalendarItem.Duration(timeService.CurrentDateTime.LocalDateTime).TotalHours * hourHeight;
            var eventTop = calculateHourOffsetFrom(startTime, hourHeight);
            var eventBottom = eventTop + durationInPx;
            
            var frameTop = scrollOffset;
            var frameBottom = Height + scrollOffset;

            if (eventTop < frameTop)
            {
                scroller.ForceFinished(true);
                isScrolling = false;
                scrollVerticallyBy((int)-(Math.Abs(frameTop - eventTop) + autoScrollToFrameExtraDistance));
                return true;
            }

            if (eventBottom > frameBottom)
            {
                scroller.ForceFinished(true);
                isScrolling = false;
                scrollVerticallyBy((int)Math.Abs(frameBottom - eventBottom) + autoScrollToFrameExtraDistance);
                return true;
            }

            return false;
        }
        
        private void scrollVerticallyBy(int deltaY)
        {
            if (isScrolling) return;
            
            scroller.ForceFinished(true);
            isScrolling = true;
            scroller.StartScroll(0, scrollOffset, 0, deltaY);
            handler.Post(continueScroll);
        }

        private void updateLayoutIfNeededDuringEdition()
        {
            var currentItemInEditMode = itemEditInEditMode;
            if (!currentItemInEditMode.IsValid) return;

            var newCalendarItem = currentItemInEditMode.CalendarItem;
            if (currentItemInEditMode.OriginalIndex == runningTimeEntryIndex)
            {
                newCalendarItem = newCalendarItem.WithDuration(null);
            }
            var newItems = calendarItems.SetItem(currentItemInEditMode.OriginalIndex, newCalendarItem);
            updateItemsAndRecalculateEventsAttrs(newItems);
        }

        private class CalendarGestureListener : GestureDetector.SimpleOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
        {
            private readonly Action<MotionEvent> onDown;
            private readonly Action<MotionEvent> onLongPress;
            private readonly Func<ScaleGestureDetector, bool> onScale;
            private readonly Action<MotionEvent, MotionEvent, float, float> onScroll;
            private readonly Action<MotionEvent, MotionEvent, float, float> onFling;
            private readonly Action<MotionEvent> onSingleTapUp;

            public CalendarGestureListener(
                Action<MotionEvent> onDown,
                Action<MotionEvent> onLongPress,
                Action<MotionEvent, MotionEvent, float, float> onScroll,
                Action<MotionEvent, MotionEvent, float, float> onFling,
                Action<MotionEvent> onSingleTapUp,
                Func<ScaleGestureDetector, bool> onScale)
            {
                this.onDown = onDown;
                this.onScale = onScale;
                this.onFling = onFling;
                this.onScroll = onScroll;
                this.onLongPress = onLongPress;
                this.onSingleTapUp = onSingleTapUp;
            }

            public override bool OnDown(MotionEvent e)
            {
                onDown(e);
                return true;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                onFling(e1, e2, velocityX, velocityY);
                return true;
            }

            public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
            {
                onScroll(e1, e2, distanceX, distanceY);
                return true;
            }

            public override bool OnSingleTapUp(MotionEvent e)
            {
                onSingleTapUp(e);
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                onLongPress(e);
            }

            public bool OnScale(ScaleGestureDetector detector)
                => onScale(detector);

            public bool OnScaleBegin(ScaleGestureDetector detector)
                => true;

            public void OnScaleEnd(ScaleGestureDetector detector)
            {
            }
        }
    }
}
