using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Accord;
using Android.Graphics;
using Android.Graphics.Fonts;
using Android.Text;
using Android.Text.Style;
using AndroidX.Core.Graphics;
using Toggl.Core.Calendar;
using Toggl.Core.Extensions;
using Toggl.Core.UI.Calendar;
using Toggl.Core.UI.Collections;
using Toggl.Core.UI.Transformations;
using Toggl.Droid.Extensions;
using Toggl.Droid.ViewHelpers;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Color = Android.Graphics.Color;
using Math = System.Math;

namespace Toggl.Droid.Views.Calendar
{
    public partial class CalendarDayView
    {
        private readonly Dictionary<string, StaticLayout> descriptionTextLayouts = new Dictionary<string, StaticLayout>();
        private readonly Dictionary<string, StaticLayout> projectTaskClientTextLayouts = new Dictionary<string, StaticLayout>();
        private readonly Dictionary<string, StaticLayout> durationTextLayouts = new Dictionary<string, StaticLayout>();
        private readonly float calendarItemColorAlpha = 0.25f;
        private readonly double minimumTextContrast = 1.6;
        private readonly RectF eventRect = new RectF();
        private readonly RectF stripeRect = new RectF();
        private readonly RectF itemInEditModeRect = new RectF();
        private readonly Paint eventsPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Paint textEventsPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Paint editingHoursLabelPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Paint iconPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Paint unsyncablePaint = new Paint(PaintFlags.AntiAlias);
        private readonly Paint shadowPaint = new Paint(0);
        private readonly PathEffect dashEffect = new DashPathEffect(new []{ 10f, 10f }, 0f);
        private readonly CalendarItemStartTimeComparer calendarItemComparer = new CalendarItemStartTimeComparer();

        private float leftMargin;
        private float leftPadding;
        private float rightPadding;
        private float itemSpacing;
        private int maxItemHeightWithoutDetails;
        private int maxItemHeightWithoutProjectTaskClient;
        private int maxItemHeightWithoutDuration;
        private float runningTimeEntryStripesSpacing;
        private float runningTimeEntryThinStripeWidth;
        private int calendarItemInterItemVerticalSpacing;
        private int calendarItemVerticalPadding;
        private int calendarItemHorizontalPadding;
        private int calendarItemDescriptionFontSize;
        private int calendarItemProjectTaskClientFontSize;
        private int calendarItemDurationFontSize;
        private int editingHandlesVerticalSpacing;
        private int editingHandlesLength;
        private Bitmap calendarIconBitmap;
        private Bitmap hasTagsIconBitmap;
        private Bitmap isBillableIconBitmap;
        private int calendarIconSize;
        private int? runningTimeEntryIndex;
        private float commonRoundRectRadius;
        private CalendarItemEditInfo itemEditInEditMode = CalendarItemEditInfo.None;
        private IDisposable runningTimeEntryDisposable;

        private float minHourHeight => hourHeight / 4f;

        public void UpdateItems(ObservableGroupedOrderedCollection<CalendarItem> calendarItems)
        {
            var newItems = calendarItems.IsEmpty
                ? ImmutableList<CalendarItem>.Empty
                : calendarItems[0].ToImmutableList();

            originalCalendarItems = newItems;
            updateItemsAndRecalculateEventsAttrs(newItems);
        }

        partial void initEventDrawingBackingFields()
        {
            leftMargin = Context.GetDimen(Resource.Dimension.calendarEventsStartMargin);
            leftPadding = Context.GetDimen(Resource.Dimension.calendarEventsLeftPadding);
            rightPadding = Context.GetDimen(Resource.Dimension.calendarEventsRightPadding);
            itemSpacing = Context.GetDimen(Resource.Dimension.calendarEventsItemsSpacing);
            availableWidth = Width - leftMargin;

            maxItemHeightWithoutDetails = Context.GetDimen(Resource.Dimension.maxItemHeightWithoutDetails);
            maxItemHeightWithoutProjectTaskClient = Context.GetDimen(Resource.Dimension.maxItemHeightWithoutProjectTaskClient);
            maxItemHeightWithoutDuration = Context.GetDimen(Resource.Dimension.maxItemHeightWithoutDuration);

            calendarItemInterItemVerticalSpacing = Context.GetDimen(Resource.Dimension.calendarItemInterItemVerticalSpacing);
            calendarItemVerticalPadding = Context.GetDimen(Resource.Dimension.calendarItemVerticalPadding);
            calendarItemHorizontalPadding = Context.GetDimen(Resource.Dimension.calendarItemHorizontalPadding);
            calendarItemDescriptionFontSize = Context.GetDimen(Resource.Dimension.calendarItemDescriptionFontSize);
            calendarItemProjectTaskClientFontSize = Context.GetDimen(Resource.Dimension.calendarItemProjectTaskClientFontSize);
            calendarItemDurationFontSize = Context.GetDimen(Resource.Dimension.calendarItemDurationFontSize);

            eventsPaint.SetStyle(Paint.Style.FillAndStroke);
            editingHoursLabelPaint.Color = Context.SafeGetColor(Resource.Color.accent);
            editingHoursLabelPaint.TextAlign = Paint.Align.Right;
            editingHoursLabelPaint.TextSize = Context.GetDimen(Resource.Dimension.editingHoursLabelPaintTextSize);
            runningTimeEntryStripesSpacing = Context.GetDimen(Resource.Dimension.calendarRunningTimeEntryStripesSpacing);
            runningTimeEntryThinStripeWidth = Context.GetDimen(Resource.Dimension.calendarRunningTimeEntryThinStripeWidth);
            commonRoundRectRadius = leftPadding / 2;
            calendarIconSize = Context.GetDimen(Resource.Dimension.calendarIconSize);
            calendarIconBitmap = Context.GetVectorDrawable(Resource.Drawable.ic_calendar).ToBitmap(calendarIconSize, calendarIconSize);
            hasTagsIconBitmap = Context.GetVectorDrawable(Resource.Drawable.ic_tag).ToBitmap(calendarIconSize, calendarIconSize);
            isBillableIconBitmap = Context.GetVectorDrawable(Resource.Drawable.ic_billable).ToBitmap(calendarIconSize, calendarIconSize);

            editingHandlesVerticalSpacing = Context.GetDimen(Resource.Dimension.editingHandlesVerticalSpacing);
            editingHandlesLength = Context.GetDimen(Resource.Dimension.editingHandlesLength);

            unsyncablePaint.SetStyle(Paint.Style.FillAndStroke);
            unsyncablePaint.Color = Context.SafeGetColor(Resource.Color.stopTimeEntryButtonBackground);

            var gradient = new LinearGradient(0, 0, 0, calendarItemVerticalPadding, Color.Transparent, Color.Black.WithOpacity(0.2f), Shader.TileMode.Repeat);
            shadowPaint.SetStyle(Paint.Style.Fill);
            shadowPaint.SetShader(gradient);
        }

        private void updateItemsAndRecalculateEventsAttrs(ImmutableList<CalendarItem> newItems)
        {
            var validItems = newItems;
            var invalidItemsCount = calendarItems.Count(item => item.Id == "");
            if (!itemEditInEditMode.IsValid && invalidItemsCount > 0)
            {
                validItems = calendarItems.Where(item => item.Id != "").ToImmutableList();
            }

            if (availableWidth > 0)
            {
                if (itemEditInEditMode.IsValid && itemEditInEditMode.HasChanged)
                    validItems = validItems.Sort(calendarItemComparer);

                calendarItemLayoutAttributes = calendarLayoutCalculator
                    .CalculateLayoutAttributes(validItems)
                    .Select(calculateCalendarItemRect)
                    .ToImmutableList();

                descriptionTextLayouts.Clear();
                projectTaskClientTextLayouts.Clear();
                durationTextLayouts.Clear();
            }

            var runningIndex = validItems.IndexOf(item => item.Duration == null);
            runningTimeEntryIndex = runningIndex >= 0 ? runningIndex : (int?)null;
            calendarItems = validItems;
            updateItemInEditMode();

            PostInvalidate();
        }

        private void updateItemInEditMode()
        {
            var currentItemInEditMode = itemEditInEditMode;
            if (!currentItemInEditMode.IsValid) return;

            var calendarItemsToSearch = calendarItems;
            var calendarItemsAttrsToSearch = calendarItemLayoutAttributes;
            var newCalendarItemInEditModeIndex = calendarItemsToSearch.IndexOf(item => item.Id == currentItemInEditMode.CalendarItem.Id);

            if (newCalendarItemInEditModeIndex < 0)
            {
                itemEditInEditMode = CalendarItemEditInfo.None;
            }
            else
            {
                var newLayoutAttr = calendarItemsAttrsToSearch[newCalendarItemInEditModeIndex];
                itemEditInEditMode = new CalendarItemEditInfo(
                    currentItemInEditMode.CalendarItem,
                    newLayoutAttr,
                    newCalendarItemInEditModeIndex,
                    hourHeight,
                    minHourHeight,
                    timeService.CurrentDateTime);
                itemEditInEditMode.CalculateRect(itemInEditModeRect);
            }
        }

        partial void processEventsOnLayout()
        {
            updateItemsAndRecalculateEventsAttrs(calendarItems);
        }

        private CalendarItemRectAttributes calculateCalendarItemRect(CalendarItemLayoutAttributes attrs)
        {
            var totalItemSpacing = (attrs.TotalColumns - 1) * itemSpacing;
            var eventWidth = (availableWidth - leftPadding - rightPadding - totalItemSpacing) / attrs.TotalColumns;
            var left = leftMargin + leftPadding + eventWidth * attrs.ColumnIndex + attrs.ColumnIndex * itemSpacing;

            return new CalendarItemRectAttributes(attrs, left, left + eventWidth);
        }

        partial void drawCalendarItems(Canvas canvas)
        {
            var itemsToDraw = calendarItems;
            var itemsAttrs = calendarItemLayoutAttributes;

            for (var index = 0; index < itemsAttrs.Count; index++)
            {
                var item = itemsToDraw[index];

                var isSelected = itemIsSelected(item);
                if (isSelected) continue;

                var itemAttrs = itemsAttrs[index];
                itemAttrs.CalculateRect(hourHeight, minHourHeight, eventRect);
                eventRect.Inset(0, calendarItemInterItemVerticalSpacing);

                if (!(eventRect.Bottom > scrollOffset) || !(eventRect.Top - scrollOffset < Height)) continue;

                var isRunning = index == runningTimeEntryIndex;

                drawCalendarItem(canvas, eventRect, item, isRunning, false);
            }

            if (itemEditInEditMode.IsValid)
            {
                drawSelectedCalendarItem(
                    canvas,
                    itemInEditModeRect,
                    itemEditInEditMode.CalendarItem,
                    runningTimeEntryIndex.HasValue ? itemEditInEditMode.OriginalIndex == runningTimeEntryIndex.Value : false);
            }

            if (runningTimeEntryIndex.HasValue && runningTimeEntryDisposable == null)
            {
                runningTimeEntryDisposable = timeService.CurrentDateTimeObservable
                    .AsDriver(default, AndroidDependencyContainer.Instance.SchedulerProvider)
                    .Subscribe(_ => Invalidate());
            }
            else if (!runningTimeEntryIndex.HasValue)
            {
                runningTimeEntryDisposable?.Dispose();
                runningTimeEntryDisposable = null;
            }
        }

        private void drawCalendarItem(Canvas canvas, RectF frame, CalendarItem item, bool isRunning, bool isSelected)
        {
            drawCalendarShape(canvas, item, frame, isRunning, isSelected);

            var isEvent = item.IconKind == CalendarIconKind.Event;
            var descriptionWidth = isEvent
                ? frame.Width() - 3 * calendarItemHorizontalPadding - calendarIconSize
                : frame.Width() - 2 * calendarItemHorizontalPadding;

            var projectTaskClientWidth = frame.Width() - 2 * calendarItemHorizontalPadding;

            var descriptionLayout = getCalendarItemDescriptionLayout(item, descriptionWidth);
            var projectTaskClientLayout = getCalendarItemProjectTaskClientLayout(item, projectTaskClientWidth);

            float descriptionTop = frame.Top + calendarItemVerticalPadding;
            float descriptionBottom;
            float projectTaskClientTop;
            float projectTaskClientBottom;

            if (frame.Height() >= maxItemHeightWithoutDetails && frame.Height() < maxItemHeightWithoutProjectTaskClient)
            {
                descriptionBottom = descriptionTop + descriptionLayout.GetLineBottom(0);
                drawCalendarItemDescription(canvas, frame, descriptionLayout, isEvent, descriptionTop, descriptionBottom);
            }
            else if (frame.Height() >= maxItemHeightWithoutProjectTaskClient && frame.Height() < maxItemHeightWithoutDuration)
            {
                descriptionBottom = descriptionTop + descriptionLayout.GetLineBottom(0);
                drawCalendarItemDescription(canvas, frame, descriptionLayout, isEvent, descriptionTop, descriptionBottom);

                projectTaskClientTop = descriptionBottom;
                projectTaskClientBottom = projectTaskClientTop + projectTaskClientLayout.GetLineBottom(0);
                drawCalendarItemProjectTaskClient(canvas, frame, projectTaskClientLayout, projectTaskClientTop, projectTaskClientBottom);
            }
            else if (frame.Height() >= maxItemHeightWithoutDuration)
            {
                var durationLayout = getCalendarItemDurationLayout(item, projectTaskClientWidth, durationFormat);
                var durationWidth = durationLayout.GetDesiredWidth();
                var durationHeight = durationLayout.Height;
                var durationTop = frame.Bottom - durationHeight;

                descriptionBottom = Math.Min(descriptionTop + descriptionLayout.Height, durationTop - calendarItemVerticalPadding);
                drawCalendarItemDescription(canvas, frame, descriptionLayout, isEvent, descriptionTop, descriptionBottom);

                projectTaskClientTop = descriptionBottom;
                projectTaskClientBottom = Math.Min(projectTaskClientTop + projectTaskClientLayout.Height, durationTop - calendarItemVerticalPadding);
                drawCalendarItemProjectTaskClient(canvas, frame, projectTaskClientLayout, projectTaskClientTop, projectTaskClientBottom);

                drawCalendarItemDuration(canvas, frame, durationLayout);
                drawCalendarItemIcons(canvas, frame, durationWidth, item.HasTags, item.IsBillable);

                var shadowOffset = frame.Bottom - durationLayout.Height - calendarItemVerticalPadding;
                var shouldDropShadow = projectTaskClientBottom >= shadowOffset;
                if (shouldDropShadow)
                {
                    drawBottomPartShadow(canvas, frame, shadowOffset);
                }
            }

            if (item.IconKind == CalendarIconKind.Unsyncable)
            {
                drawUnsyncableBadge(canvas, frame);
            }
        }

        private void drawSelectedCalendarItem(Canvas canvas, RectF frame, CalendarItem item, bool isRunning)
        {
            drawCalendarItem(canvas, frame, item, isRunning, true);
            drawEditingHandles(canvas, frame, isRunning);
        }

        private bool itemIsSelected(CalendarItem calendarItem)
            => calendarItem.Id == itemEditInEditMode.CalendarItem.Id;

        #region Draw background

        private void drawCalendarShape(Canvas canvas, CalendarItem item, RectF frame, bool isRunning, bool isSelected)
        {
            if (isRunning)
            {
                drawRunningTimeEntryCalendarItemShape(canvas, item, frame);
            }
            else
            {
                drawRegularCalendarItemShape(canvas, item, frame, isSelected);
            }
        }

        private void drawRegularCalendarItemShape(Canvas canvas, CalendarItem item, RectF calendarItemRect, bool isSelected)
        {
            eventsPaint.SetStyle(Paint.Style.FillAndStroke);
            eventsPaint.Color = Context.SafeGetColor(Resource.Color.calendarItemBackground);
            canvas.DrawRoundRect(calendarItemRect, commonRoundRectRadius, commonRoundRectRadius, eventsPaint);

            if (!item.IsPlaceholder)
            {
                var alpha = isSelected ? 0.34f : 0.24f;
                var color = Color.ParseColor(item.Color).WithOpacity(alpha);
                eventsPaint.SetStyle(Paint.Style.FillAndStroke);
                eventsPaint.Color = color;
                canvas.DrawRoundRect(calendarItemRect, commonRoundRectRadius, commonRoundRectRadius, eventsPaint);
            }
        }

        private void drawRunningTimeEntryCalendarItemShape(Canvas canvas, CalendarItem item, RectF calendarItemRect)
        {
            var itemColor = Color.ParseColor(item.Color);
            var calendarFillColor = new Color(itemColor);
            calendarFillColor.A = (byte) (calendarFillColor.A * 0.05f);
            var bgColor = Context.SafeGetColor(Resource.Color.cardBackground);
            calendarFillColor = new Color(ColorUtils.CompositeColors(calendarFillColor, bgColor));

            var calendarStripeColor = new Color(itemColor);
            calendarStripeColor.A = (byte) (calendarStripeColor.A * 0.1f);

            drawShapeBaseBackgroundFilling(calendarItemRect, calendarFillColor);
            drawShapeBackgroundStripes(calendarItemRect, calendarStripeColor);
            drawSolidBorder(calendarItemRect, itemColor);

            void drawShapeBaseBackgroundFilling(RectF rectF, Color color)
            {
                eventsPaint.Color = color;
                eventsPaint.SetStyle(Paint.Style.FillAndStroke);
                canvas.DrawRoundRect(rectF, commonRoundRectRadius, commonRoundRectRadius, eventsPaint);
            }

            void drawShapeBackgroundStripes(RectF shapeRect, Color color)
            {
                canvas.Save();
                canvas.ClipRect(shapeRect);
                canvas.Rotate(45f, shapeRect.Left, shapeRect.Top);
                eventsPaint.Color = color;
                var hyp = (float) Math.Sqrt(Math.Pow(shapeRect.Height(), 2) + Math.Pow(shapeRect.Width(), 2));
                stripeRect.Set(shapeRect.Left, shapeRect.Top - hyp, shapeRect.Left + runningTimeEntryThinStripeWidth, shapeRect.Bottom + hyp);
                for (var stripeStart = 0f; stripeStart < hyp; stripeStart += runningTimeEntryStripesSpacing)
                {
                    stripeRect.Set(shapeRect.Left + stripeStart, stripeRect.Top, shapeRect.Left + stripeStart + runningTimeEntryThinStripeWidth, stripeRect.Bottom);
                    canvas.DrawRect(stripeRect, eventsPaint);
                }

                canvas.Restore();
            }

            void drawSolidBorder(RectF borderRect, Color color)
            {
                eventsPaint.SetStyle(Paint.Style.Stroke);
                eventsPaint.StrokeWidth = 1.DpToPixels(Context);
                eventsPaint.Color = color;
                eventsPaint.SetPathEffect(dashEffect);
                canvas.DrawRoundRect(borderRect, commonRoundRectRadius, commonRoundRectRadius, eventsPaint);
                eventsPaint.SetPathEffect(null);
            }
        }

        private void drawUnsyncableBadge(Canvas canvas, RectF frame)
        {
            var radius = calendarIconSize / 2;
            canvas.Save();
            canvas.Translate(frame.Right - radius, frame.Bottom - radius);
            canvas.ClipRect(0, 0, radius, radius);
            canvas.DrawCircle(frame.Right, frame.Bottom, radius, unsyncablePaint);
            canvas.Restore();
        }

        #endregion

        #region Draw texts

        private void drawCalendarItemDescription(Canvas canvas, RectF frame, StaticLayout textLayout, bool isEvent, float top, float bottom)
        {
            var textLeft = isEvent ? frame.Left + calendarIconSize : frame.Left + calendarItemHorizontalPadding;
            if (isEvent)
            {
                iconPaint.SetColorFilter(
                    new PorterDuffColorFilter(new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText)),
                        PorterDuff.Mode.SrcIn));
                canvas.DrawBitmap(calendarIconBitmap, frame.Left, frame.Top + calendarItemVerticalPadding, iconPaint);
            }

            canvas.Save();
            canvas.Translate(textLeft, top);
            canvas.ClipRect(0, 0, frame.Width() - calendarItemHorizontalPadding, bottom - top);
            textLayout.Draw(canvas);
            canvas.Restore();
        }

        private void drawCalendarItemProjectTaskClient(Canvas canvas, RectF frame, StaticLayout textLayout, float top, float bottom)
        {
            canvas.Save();
            canvas.Translate(frame.Left + calendarItemHorizontalPadding, top);
            canvas.ClipRect(0, 0, frame.Width() - calendarItemHorizontalPadding, bottom - top);
            textLayout.Draw(canvas);
            canvas.Restore();
        }

        private void drawCalendarItemDuration(Canvas canvas, RectF frame, StaticLayout textLayout)
        {
            var lineHeight = textLayout.Height;
            canvas.Save();
            canvas.Translate(frame.Left + calendarItemHorizontalPadding, frame.Bottom - lineHeight - calendarItemVerticalPadding);
            canvas.ClipRect(0, 0, frame.Width() - calendarItemHorizontalPadding, lineHeight);
            textLayout.Draw(canvas);
            canvas.Restore();
        }

        private void drawCalendarItemIcons(Canvas canvas, RectF frame, float durationWidth, bool hasTags, bool isBillable)
        {
            if (durationWidth + 3 * calendarItemHorizontalPadding + 2 * calendarIconSize >= frame.Width())
                return;

            var hasTagsStart = isBillable
                ? frame.Right - 2 * calendarItemHorizontalPadding - 2 * calendarIconSize
                : frame.Right - calendarItemHorizontalPadding - calendarIconSize;

            var isBillableStart = frame.Right - calendarItemHorizontalPadding - calendarIconSize;

            iconPaint.SetColorFilter(
                new PorterDuffColorFilter(new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText)),
                PorterDuff.Mode.SrcIn));

            if (hasTags)
            {
                canvas.DrawBitmap(hasTagsIconBitmap, hasTagsStart, frame.Bottom - calendarIconSize - calendarItemVerticalPadding, iconPaint);
            }

            if (isBillable)
            {
                canvas.DrawBitmap(isBillableIconBitmap, isBillableStart, frame.Bottom - calendarIconSize - calendarItemVerticalPadding, iconPaint);
            }
        }

        private void drawBottomPartShadow(Canvas canvas, RectF frame, float offset)
        {
            canvas.Save();
            canvas.Translate(frame.Left, offset);
            canvas.ClipRect(0, 0, frame.Width(), calendarItemVerticalPadding);
            canvas.DrawPaint(shadowPaint);
            canvas.Restore();
        }

        #endregion

        #region Text layouts

        private StaticLayout getCalendarItemDescriptionLayout(CalendarItem item, float eventWidth)
        {
            var spannable = getCalendarItemDescriptionSpannable(item);
            var text = spannable.ToString();

            descriptionTextLayouts.TryGetValue(item.Id, out var eventTextLayout);
            if (eventTextLayout != null && !(Math.Abs(eventTextLayout.Width - eventWidth) > 0.1) && eventTextLayout.Text == text)
                return eventTextLayout;

            var color = new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText));

            textEventsPaint.Color = color;
            textEventsPaint.TextSize = calendarItemDescriptionFontSize;

            eventTextLayout = new StaticLayout(spannable,
                0,
                text.Length,
                new TextPaint(textEventsPaint),
                (int) eventWidth,
                Android.Text.Layout.Alignment.AlignNormal,
                1.0f,
                0.0f,
                true,
                TextUtils.TruncateAt.End,
                (int) eventWidth);
            descriptionTextLayouts[item.Id] = eventTextLayout;

            return eventTextLayout;
        }

        private SpannableString getCalendarItemDescriptionSpannable(CalendarItem item)
        {
            var color = new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText));
            var text = item.Description;
            var spannable = new SpannableString(text);

            spannable.SetSpan(
                new ForegroundColorSpan(color),
                0, text.Length,
                SpanTypes.InclusiveInclusive);
            spannable.SetSpan(
                new TypefaceSpan("sans-serif-medium"),
                0, text.Length,
                SpanTypes.InclusiveInclusive);

            return spannable;
        }

        private StaticLayout getCalendarItemProjectTaskClientLayout(CalendarItem item, float eventWidth)
        {
            var spannable = getCalendarItemProjectTaskClientSpannable(item);
            var text = spannable.ToString();
            projectTaskClientTextLayouts.TryGetValue(item.Id, out var eventTextLayout);
            if (eventTextLayout != null && !(Math.Abs(eventTextLayout.Width - eventWidth) > 0.1) && eventTextLayout.Text == text)
                return eventTextLayout;

            var color = Color.ParseColor(item.Color);

            textEventsPaint.Color = color;
            textEventsPaint.TextSize = calendarItemProjectTaskClientFontSize;

            eventTextLayout = new StaticLayout(spannable,
                0,
                text.Length,
                new TextPaint(textEventsPaint),
                (int) eventWidth,
                Android.Text.Layout.Alignment.AlignNormal,
                1.0f,
                0.0f,
                true,
                TextUtils.TruncateAt.End,
                (int) eventWidth);
            projectTaskClientTextLayouts[item.Id] = eventTextLayout;

            return eventTextLayout;
        }

        private SpannableString getCalendarItemProjectTaskClientSpannable(CalendarItem item)
        {
            var projectColor = Color.ParseColor(item.Color);
            var clientColor = new Color(Context.SafeGetColor(Resource.Color.calendarItemSecondaryText));
            var hasClient = !string.IsNullOrEmpty(item.Client);

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(item.Project))
                builder.Append($"{item.Project}");

            if (!string.IsNullOrEmpty(item.Task))
                builder.Append($": {item.Task}");

            var clientIndex = builder.Length;

            if (hasClient)
                builder.Append($" · {item.Client}");

            var text = builder.ToString();
            var spannable = new SpannableString(text);

            spannable.SetSpan(
                new ForegroundColorSpan(projectColor),
                0, clientIndex,
                SpanTypes.InclusiveInclusive);

            if (hasClient)
            {
                spannable.SetSpan(
                    new ForegroundColorSpan(clientColor),
                    clientIndex, text.Length,
                    SpanTypes.InclusiveInclusive);}

            return spannable;
        }

        private StaticLayout getCalendarItemDurationLayout(CalendarItem item, float eventWidth, DurationFormat durationFormat)
        {
            var spannable = getCalendarItemDurationSpannable(item, durationFormat);
            var text = spannable.ToString();
            durationTextLayouts.TryGetValue(item.Id, out var layout);
            if (layout != null && !(Math.Abs(layout.Width - eventWidth) > 0.1) && layout.Text == text)
                return layout;

            var color = new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText));

            textEventsPaint.Color = color;
            textEventsPaint.TextSize = calendarItemDurationFontSize;

            layout = new StaticLayout(spannable,
                0,
                text.Length,
                new TextPaint(textEventsPaint),
                (int) eventWidth,
                Android.Text.Layout.Alignment.AlignNormal,
                1.0f,
                0.0f,
                true,
                TextUtils.TruncateAt.End,
                (int) eventWidth);
            durationTextLayouts[item.Id] = layout;

            return layout;
        }

        private SpannableString getCalendarItemDurationSpannable(CalendarItem item, DurationFormat durationFormat)
        {
            var duration = item.Duration ?? timeService.CurrentDateTime - item.StartTime;
            var format = item.Duration.HasValue ? durationFormat : DurationFormat.Improved;
            var formattedDuration = DurationAndFormatToString.Convert(duration, format);
            var color = new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText));
            var spannable = new SpannableString(formattedDuration);

            spannable.SetSpan(
                new ForegroundColorSpan(color),
                0, formattedDuration.Length,
                SpanTypes.InclusiveInclusive);

            return spannable;
        }

        #endregion

        #region Draw handles

        private void drawEditingHandles(Canvas canvas, RectF frame, bool isRunning)
        {
            eventsPaint.Color = new Color(Context.SafeGetColor(Resource.Color.calendarItemPrimaryText));
            eventsPaint.SetStyle(Paint.Style.FillAndStroke);

            var centerX = frame.CenterX();

            canvas.DrawRect(
                centerX - editingHandlesLength,
                frame.Top + editingHandlesVerticalSpacing,
                centerX + editingHandlesLength,
                frame.Top + 2 * editingHandlesVerticalSpacing,
                eventsPaint);

            canvas.DrawRect(
                centerX - editingHandlesLength,
                frame.Top + 3 * editingHandlesVerticalSpacing,
                centerX + editingHandlesLength,
                frame.Top + 4 * editingHandlesVerticalSpacing,
                eventsPaint);
            canvas.DrawText(startHourLabel, hoursX, frame.Top + editingHoursLabelPaint.Descent(), editingHoursLabelPaint);

            if (!isRunning)
            {
                canvas.DrawRect(
                    centerX - editingHandlesLength,
                    frame.Bottom - 4 * editingHandlesVerticalSpacing,
                    centerX + editingHandlesLength,
                    frame.Bottom - 3 * editingHandlesVerticalSpacing,
                    eventsPaint);

                canvas.DrawRect(
                    centerX - editingHandlesLength,
                    frame.Bottom - 2 * editingHandlesVerticalSpacing,
                    centerX + editingHandlesLength,
                    frame.Bottom - editingHandlesVerticalSpacing,
                    eventsPaint);
            }
            canvas.DrawText(endHourLabel, hoursX, frame.Bottom + editingHoursLabelPaint.Descent(), editingHoursLabelPaint);
        }

        #endregion

        private sealed class CalendarItemStartTimeComparer : Comparer<CalendarItem>
        {
            public override int Compare(CalendarItem x, CalendarItem y)
                => x.StartTime.LocalDateTime.CompareTo(y.StartTime.LocalDateTime);
        }
    }
}
