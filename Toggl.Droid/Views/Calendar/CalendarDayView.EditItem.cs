using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Animation;
using Android.Graphics;
using Android.Views;
using Toggl.Core.Calendar;
using Toggl.Core.Extensions;
using Toggl.Droid.Extensions;
using Toggl.Droid.Helper;
using Toggl.Droid.ViewHelpers;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Views.Calendar
{
    public partial class CalendarDayView
    {
        private readonly RectF dragTopRect = new RectF();
        private readonly RectF dragBottomRect = new RectF();
        private readonly RectF eventStartingRect = new RectF();
        private const float dragAcceleration = 0.1f;
        private const float dragMaxSpeed = 15f;

        private string startHourLabel = string.Empty;
        private string endHourLabel = string.Empty;
        private int handleTouchExtraMargins;
        private bool shouldTryToAutoScrollToEvent;
        private DateTimeOffset previousStartTime;
        private DateTimeOffset previousEndTime;
        private EditAction editAction = EditAction.None;
        private ValueAnimator autoScrollAnimator;
        private bool autoScrollWasCancelled;
        private bool isDragging;
        private float draggingScrollDelta;
        private float draggingDelta;
        private float draggingSpeed;
        private float currentTouchY;
        private float dragStartingTouchY;
        private float dragStartingScrollOffset;
        private int draggingDirection;

        partial void initEventEditionBackingFields()
        {
            handleTouchExtraMargins = Context.GetDimen(Resource.Dimension.calendarEditingHandleTouchExtraMargins);
            autoScrollAnimator = ValueAnimator.OfFloat(0f, 1f);
            autoScrollAnimator.SetDuration(240);
            autoScrollAnimator.AnimationStart += (sender, args) => onAnimationStarted();
            autoScrollAnimator.Update += (sender, args) => continueAutoScroll();
            autoScrollAnimator.AnimationEnd += (sender, args) => restartAutoScroll();
            autoScrollAnimator.AnimationCancel += (sender, args) => onAnimationCancelled();
        }

        private void onAnimationStarted()
        {
            autoScrollWasCancelled = false;
        }

        private void onAnimationCancelled()
        {
            autoScrollWasCancelled = true;
        }

        private void restartAutoScroll()
        {
            if (autoScrollWasCancelled || !isDragging || autoScrollAnimator.IsRunning)
                return;

            if (scrollOffset > 0 && scrollOffset < maxHeight - Height && itemInEditModeRect.Top > 0 && itemInEditModeRect.Bottom < maxHeight)
            {
                autoScrollAnimator.Start();
            }
        }

        private void continueAutoScroll()
        {
            if (!isDragging)
                return;

            draggingSpeed = (draggingSpeed + dragAcceleration * draggingDirection).Clamp(-dragMaxSpeed, dragMaxSpeed);
            scrollOffsetSubject.OnNext((int) (scrollOffset + draggingSpeed).Clamp(0f, maxHeight - Height));
            draggingScrollDelta = scrollOffset - dragStartingScrollOffset;

            if (draggingDirection == -1 && scrollOffset <= 0 || draggingDirection == 1 && scrollOffset >= maxHeight - Height)
            {
                cancelDraggingAndAutoScroll();
                PostInvalidate();
                return;
            }

            switch (editAction)
            {
                case EditAction.ChangeStart:
                    updateItemInEditModeStartTime();
                    break;
                case EditAction.ChangeEnd:
                    updateItemInEditModeEndTime();
                    break;
                case EditAction.ChangeOffset:
                    updateItemInEditModeOffset();
                    break;
            }

            PostInvalidate();
        }

        private void cancelDraggingAndAutoScroll()
        {
            draggingSpeed = 0;
            draggingDirection = 0;
            if (autoScrollAnimator.IsRunning)
            {
                autoScrollAnimator.Cancel();
            }
            Invalidate();
        }

        private void onTouchDownWhileEditingItem(MotionEvent e1)
        {
            var activeItemInfo = itemEditInEditMode;
            if (!activeItemInfo.IsValid) return;

            calculateTopDragRect(activeItemInfo);
            calculateBottomDragRect(activeItemInfo);
            activeItemInfo.CalculateRect(tapCheckRectF);

            var touchX = e1.GetX();
            var touchY = e1.GetY();

            dragStartingTouchY = touchY;
            dragStartingScrollOffset = scrollOffset;
            eventStartingRect.Set(itemInEditModeRect);

            if (dragTopRect.Contains(touchX, touchY + scrollOffset))
            {
                editAction = EditAction.ChangeStart;
            }
            else if (activeItemInfo.OriginalIndex == runningTimeEntryIndex)
            {
                editAction = EditAction.None;
            }
            else if (dragBottomRect.Contains(touchX, touchY + scrollOffset))
            {
                editAction = EditAction.ChangeEnd;
            }
            else if (tapCheckRectF.Contains(touchX, touchY + scrollOffset))
            {
                editAction = EditAction.ChangeOffset;
            }
            else
            {
                editAction = EditAction.None;
            }

            isDragging = editAction != EditAction.None;
        }

        private void calculateTopDragRect(CalendarItemEditInfo activeItemEditInfo)
        {
            activeItemEditInfo.CalculateRect(dragTopRect);
            dragTopRect.Bottom = dragTopRect.Top + handleTouchExtraMargins;
            dragTopRect.Top = dragTopRect.Top - handleTouchExtraMargins;
            dragTopRect.Left = dragTopRect.CenterX() - handleTouchExtraMargins;
            dragTopRect.Right = dragTopRect.CenterX() + handleTouchExtraMargins;
        }

        private void calculateBottomDragRect(CalendarItemEditInfo activeItemEditInfo)
        {
            activeItemEditInfo.CalculateRect(dragBottomRect);
            dragBottomRect.Top = dragBottomRect.Bottom - handleTouchExtraMargins;
            dragBottomRect.Bottom = dragBottomRect.Bottom + handleTouchExtraMargins;
            dragBottomRect.Left = dragBottomRect.CenterX() - handleTouchExtraMargins;
            dragBottomRect.Right = dragBottomRect.CenterX() + handleTouchExtraMargins;
        }

        private void dragEvent(MotionEvent e)
        {
            var calendarItemInfoInEditMode = itemEditInEditMode;
            if (!calendarItemInfoInEditMode.IsValid) return;

            var histCount = e.HistorySize;
            var avgYtouch = 0f;
            for (var i = 0; i < histCount; i++)
            {
                avgYtouch += e.GetHistoricalY(0, i);
            }

            var touchY = histCount > 0 ? avgYtouch / histCount : e.GetY();
            currentTouchY = touchY;
            draggingDelta = touchY - dragStartingTouchY;
            draggingScrollDelta = scrollOffset - dragStartingScrollOffset;
            switch (editAction)
            {
                case EditAction.ChangeStart:
                    updateItemInEditModeStartTime();
                    break;

                case EditAction.ChangeEnd:
                    updateItemInEditModeEndTime();
                    break;

                case EditAction.ChangeOffset:
                    updateItemInEditModeOffset();
                    break;
            }

            if (shouldAutoScrollUp())
            {
                draggingDirection = -1;
            }
            else if (shouldAutoScrollDown())
            {
                draggingDirection = 1;
            }
            else
            {
                cancelDraggingAndAutoScroll();
            }

            if (draggingDirection != 0 && !autoScrollAnimator.IsRunning)
                autoScrollAnimator.Start();

            Invalidate();
        }

        private bool shouldAutoScrollUp()
            => currentTouchY + scrollOffset < topAreaTriggerLine
               && scrollOffset > 0
               && itemInEditModeRect.Top > 0;

        private bool shouldAutoScrollDown()
            => currentTouchY + scrollOffset > bottomAreaTriggerLine
               && scrollOffset < maxHeight - Height
               && itemInEditModeRect.Bottom < maxHeight;

        private void updateItemInEditModeStartTime()
        {
            if (!itemEditInEditMode.IsValid)
                return;

            var newTop = (eventStartingRect.Top + draggingDelta + draggingScrollDelta).Clamp(0f, itemInEditModeRect.Bottom);
            itemInEditModeRect.Top = newTop;

            var itemToEdit = itemEditInEditMode;
            var calendarItem = itemToEdit.CalendarItem;

            var now = timeService.CurrentDateTime;

            var newStartTime = newStartTimeWithDynamicDuration(itemInEditModeRect.Top, allItemsStartAndEndTime);
            var newDuration = calendarItem.Duration.HasValue ? calendarItem.EndTime(now) - newStartTime : null as TimeSpan?;

            if (newDuration != null && newDuration <= TimeSpan.Zero ||
                newDuration == null && newStartTime > now)
                return;

            calendarItem = calendarItem
                .WithStartTime(newStartTime)
                .WithDuration(newDuration);

            updateItemInEditMode(calendarItem.StartTime, calendarItem.Duration(now));

            if (previousStartTime != newStartTime)
            {
                vibrate();
                previousStartTime = newStartTime;
                shouldTryToAutoScrollToEvent = true;
            }

            if (newTop <= 0)
            {
                cancelDraggingAndAutoScroll();
            }
        }

        private void updateItemInEditModeEndTime()
        {
            if (!itemEditInEditMode.IsValid)
                return;

            var itemToEdit = itemEditInEditMode;
            if (itemToEdit.CalendarItem.Duration == null)
                return;

            var newBottom = (eventStartingRect.Bottom + draggingDelta + draggingScrollDelta).Clamp(itemInEditModeRect.Top, maxHeight);
            itemInEditModeRect.Bottom = newBottom - calendarItemInterItemVerticalSpacing;

            var now = timeService.CurrentDateTime;
            var calendarItem = itemToEdit.CalendarItem;

            var newEndTime = newEndTimeWithDynamicDuration(itemInEditModeRect.Bottom, allItemsStartAndEndTime);
            var newDuration = newEndTime - calendarItem.StartTime;

            if (newDuration <= TimeSpan.Zero || newEndTime >= currentDate.AddDays(1))
                return;

            calendarItem = calendarItem
                .WithStartTime(calendarItem.StartTime.ToLocalTime())
                .WithDuration(newDuration);

            updateItemInEditMode(calendarItem.StartTime, calendarItem.Duration(now));

            if (previousEndTime != newEndTime)
            {
                vibrate();
                previousEndTime = newEndTime;
                shouldTryToAutoScrollToEvent = true;
            }

            if (newBottom >= maxHeight)
                cancelDraggingAndAutoScroll();
        }

        private void updateItemInEditModeOffset()
        {
            if (!itemEditInEditMode.IsValid)
                return;

            var itemToEdit = itemEditInEditMode;
            var calendarItem = itemToEdit.CalendarItem;

            var newTop = eventStartingRect.Top + draggingDelta + draggingScrollDelta;
            var newBottom = eventStartingRect.Bottom + draggingDelta + draggingScrollDelta;

            if (newTop <= 0 || newBottom >= maxHeight)
            {
                cancelDraggingAndAutoScroll();
                return;
            }

            itemInEditModeRect.Top = newTop;
            itemInEditModeRect.Bottom = newBottom;
            var newStartTime = newStartTimeWithStaticDuration(itemInEditModeRect.Top, allItemsStartAndEndTime, calendarItem.Duration);

            var now = timeService.CurrentDateTime;

            if (newStartTime + calendarItem.Duration >= currentDate.AddDays(1))
                return;

            calendarItem = calendarItem
                .WithStartTime(newStartTime);

            updateItemInEditMode(calendarItem.StartTime.ToLocalTime(), calendarItem.Duration(now));

            if (previousStartTime != newStartTime)
            {
                vibrate();
                previousStartTime = newStartTime;
                shouldTryToAutoScrollToEvent = true;
            }
        }

        private void updateItemInEditMode(DateTimeOffset startTime, TimeSpan duration)
        {
            var newCalendarItem = itemEditInEditMode.CalendarItem
                .WithStartTime(startTime.ToLocalTime())
                .WithDuration(duration);

            itemEditInEditMode = itemEditInEditMode.WithCalendarItem(newCalendarItem, hourHeight, minHourHeight, timeService.CurrentDateTime);

            updateEditingStartEndLabels();
            notifyUpdateInItemInEditMode();
        }

        private void notifyUpdateInItemInEditMode()
        {
            if (itemEditInEditMode.OriginalIndex != runningTimeEntryIndex)
            {
                calendarItemTappedSubject.OnNext(itemEditInEditMode.CalendarItem);
                return;
            }

            calendarItemTappedSubject.OnNext(itemEditInEditMode.CalendarItem.WithDuration(null));
        }

        private void updateEditingStartEndLabels()
        {
            var calendarItem = itemEditInEditMode.CalendarItem;
            var startHour = calendarItem.StartTime.ToLocalTime();
            startHourLabel = formatEditingHour(startHour.DateTime);
            if (calendarItem.EndTime.HasValue)
            {
                var endHour = calendarItem.EndTime.Value.ToLocalTime();
                endHourLabel = formatEditingHour(endHour.DateTime);
            }
        }

        private string formatEditingHour(DateTime hour)
            => hour.ToString(editingHoursFormat(), CultureInfo.CurrentCulture);

        private string editingHoursFormat()
            => timeOfDayFormat.IsTwentyFourHoursFormat
                ? Shared.Resources.EditingTwentyFourHoursFormat
                : Shared.Resources.EditingTwelveHoursFormat;

        private DateTimeOffset newStartTimeWithDynamicDuration(float yOffset, IList<DateTimeOffset> currentItemsStartAndEndTimes)
        {
            if (!currentItemsStartAndEndTimes.Any())
            {
                return dateAtYOffset(yOffset).ToLocalTime().RoundToClosestQuarter();
            }

            var newStartTime = dateAtYOffset(yOffset).ToLocalTime();

            var startSnappingPointDifference = distanceToClosestSnappingPoint(newStartTime, currentItemsStartAndEndTimes.Append(newStartTime.RoundToClosestQuarter()));

            newStartTime += startSnappingPointDifference;

            return newStartTime;
        }

        private DateTimeOffset newEndTimeWithDynamicDuration(float yOffset, IList<DateTimeOffset> currentItemsStartAndEndTimes)
        {
            if (!currentItemsStartAndEndTimes.Any())
            {
                return dateAtYOffset(yOffset).ToLocalTime().RoundToClosestQuarter();
            }

            var newEndTime = dateAtYOffset(yOffset).ToLocalTime();

            var endSnappingPointDifference = distanceToClosestSnappingPoint((DateTimeOffset) newEndTime, currentItemsStartAndEndTimes.Append(newEndTime.RoundToClosestQuarter()));

            newEndTime += endSnappingPointDifference;

            return newEndTime;
        }

        private DateTimeOffset newStartTimeWithStaticDuration(float yOffset, IList<DateTimeOffset> currentItemsStartAndEndTimes, TimeSpan? duration)
        {
            if (!currentItemsStartAndEndTimes.Any())
            {
                return dateAtYOffset(yOffset).ToLocalTime().RoundToClosestQuarter();
            }

            var newStartTime = dateAtYOffset(yOffset).ToLocalTime();
            var newEndTime = duration.HasValue ? newStartTime + duration : null;

            var startSnappingPointDifference = distanceToClosestSnappingPoint(newStartTime, currentItemsStartAndEndTimes.Append(newStartTime.RoundToClosestQuarter()));

            if (newEndTime.HasValue)
            {
                var endSnappingPointDifference = distanceToClosestSnappingPoint((DateTimeOffset) newEndTime, currentItemsStartAndEndTimes);
                var snappingPointDifference = startSnappingPointDifference.Positive() < endSnappingPointDifference.Positive() ? startSnappingPointDifference : endSnappingPointDifference;
                newStartTime += snappingPointDifference;
            }
            else
            {
                newStartTime += startSnappingPointDifference;
            }

            return newStartTime;
        }

        private TimeSpan distanceToClosestSnappingPoint(DateTimeOffset time, IEnumerable<DateTimeOffset> data)
            => data.Aggregate(TimeSpan.MaxValue,
                (min, next) => min.Positive() <= (next - time).Positive()
                    ? min
                    : next - time);

        private DateTimeOffset dateAtYOffset(float y)
        {
            var seconds = (y / hourHeight) * 60 * 60;
            var timespan = TimeSpan.FromSeconds(seconds);
            var nextDay = currentDate.AddDays(1);

            var offset = currentDate + timespan;

            if (offset < currentDate)
                return currentDate;
            if (offset > nextDay)
                return nextDay;

            return currentDate + timespan;
        }

        private void vibrate()
        {
            hapticFeedbackProvider?.ActivateVibration(vibrationDurationInMilliseconds, vibrationAmplitude);
        }

        private static float calculateHourOffsetFrom(DateTimeOffset dateTimeOffset, float hourHeight)
        {
            return (dateTimeOffset.Hour + dateTimeOffset.Minute / 60f) * hourHeight;
        }

        private enum EditAction
        {
            ChangeStart,
            ChangeEnd,
            ChangeOffset,
            None
        }

        private struct CalendarItemEditInfo
        {
            public CalendarItem CalendarItem { get; private set; }
            public int OriginalIndex { get; private set; }
            public bool IsValid { get; private set; }
            public bool HasChanged { get; private set; }

            private float top;
            private float bottom;
            private float left;
            private float right;

            public static readonly CalendarItemEditInfo None = new CalendarItemEditInfo { IsValid = false };

            public CalendarItemEditInfo(CalendarItem calendarItem, CalendarItemRectAttributes attributes, int originalIndex, float hourHeight, float minHeight, DateTimeOffset now)
            {
                var startTime = calendarItem.StartTime.LocalDateTime;
                var duration = calendarItem.Duration(now);
                CalendarItem = calendarItem;
                OriginalIndex = originalIndex;
                IsValid = true;
                HasChanged = false;
                top = calculateHourOffsetFrom(startTime, hourHeight);
                bottom = calculateHourOffsetFrom(startTime + duration, hourHeight);
                left = attributes.Left;
                right = attributes.Right;

                enforceMinHeight(minHeight);
            }

            public CalendarItemEditInfo WithCalendarItem(CalendarItem calendarItem, float hourHeight, float minHeight, DateTimeOffset now)
            {
                var startTime = calendarItem.StartTime.LocalDateTime;
                var duration = calendarItem.Duration(now);
                var newCalendarItemEditInfo = new CalendarItemEditInfo
                {
                    CalendarItem = calendarItem,
                    OriginalIndex = OriginalIndex,
                    IsValid = true,
                    HasChanged = true,
                    top = calculateHourOffsetFrom(startTime, hourHeight),
                    bottom = calculateHourOffsetFrom(startTime + duration, hourHeight),
                    left = left,
                    right = right
                };
                newCalendarItemEditInfo.enforceMinHeight(minHeight);

                return newCalendarItemEditInfo;
            }

            private void enforceMinHeight(float minHeight)
            {
                if (bottom - top < minHeight)
                {
                    bottom = top + minHeight;
                }
            }

            public void CalculateRect(RectF outRectF)
            {
                outRectF.Set(left, top, right, bottom);
            }
        }
    }
}
