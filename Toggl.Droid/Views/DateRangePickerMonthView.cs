using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Toggl.Core.UI.ViewModels.DateRangePicker;
using Toggl.Droid.Extensions;
using Toggl.Shared.Extensions.Reactive;

namespace Toggl.Droid.Views
{
    [Register("toggl.droid.views.DateRangePickerMonthView")]
    public class DateRangePickerMonthView : View
    {
        public const int RowHeightDp = 42;
        private const float selectionVerticalRatio = 0.8f;

        private DateRangePickerMonthInfo monthInfo;
        private int rowHeight;
        private int dayWidth;
        private int radius;
        private int selectionHeight;
        private int verticalPadding;

        private Paint backgroundPaint = new Paint() { AntiAlias = true, StrokeWidth = 4 };
        private Paint textPaint = new Paint() { AntiAlias = true, TextAlign = Paint.Align.Center };
        private Rect textBounds = new Rect();
        private BehaviorRelay<DateTime?> dateSelectedRelay;

        private Color backgroundColor;
        private Color monthTextColor;
        private Color extraMonthTextColor;
        private Color selectedTextColor;
        private Color todayMarkerBackgroundColor;
        private Color selectionBackgroundColor;

        public DateRangePickerMonthView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public DateRangePickerMonthView(Context context)
            : base(context)
        {
            initialize(context);
        }

        public DateRangePickerMonthView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            initialize(context);
        }

        private void initialize(Context context)
        {
            backgroundColor = context.SafeGetColor(Resource.Color.dateRangePickerBackground);
            monthTextColor = context.SafeGetColor(Resource.Color.monthTextColor);
            extraMonthTextColor = context.SafeGetColor(Resource.Color.extraMonthTextColor);
            selectedTextColor = context.SafeGetColor(Resource.Color.selectedTextColor);
            todayMarkerBackgroundColor = context.SafeGetColor(Resource.Color.todayMarkerBackground);
            selectionBackgroundColor = context.SafeGetColor(Resource.Color.selectionBackground);

            rowHeight = RowHeightDp.DpToPixels(context);
            textPaint.TextSize = 14.DpToPixels(context);

            radius = (int)(rowHeight * selectionVerticalRatio / 2);
            selectionHeight = 2 * radius;
            verticalPadding = (rowHeight - selectionHeight) / 2;
        }

        public void Setup(DateRangePickerMonthInfo monthInfo, BehaviorRelay<DateTime?> dateSelectedRelay)
        {
            this.dateSelectedRelay = dateSelectedRelay;
            this.monthInfo = monthInfo;
            PostInvalidate();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e.Action != MotionEventActions.Up)
                return base.OnTouchEvent(e);

            var viewLocation = this.GetLocationOnScreen();

            var column = (int)(e.RawX - viewLocation.X) / dayWidth;
            var row = (int)(e.RawY - viewLocation.Y) / rowHeight;

            var index = row * 7 + column;

            var selectedDate = monthInfo.DisplayDates.Beginning.AddDays(index);
            dateSelectedRelay.Accept(selectedDate);

            return false;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (monthInfo == null)
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            var size = MeasureSpec.GetSize(widthMeasureSpec);

            var rows = monthInfo.DisplayDates.Length / 7;
            var totalHeight = rows * rowHeight;

            dayWidth = size / 7;

            SetMeasuredDimension(size, totalHeight);
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.Save();
            performCanvasDriftCorrection(canvas);

            drawBackground(canvas);

            var date = monthInfo.DisplayDates.Beginning;
            for (int i = 0; date <= monthInfo.DisplayDates.End; i++, date = date.AddDays(1))
            {
                var row = i / 7;
                var column = i % 7;

                var x = column * dayWidth;
                var y = row * rowHeight;

                canvas.Save();
                canvas.Translate(x, y);
                drawDay(canvas, date);
                canvas.Restore();
            }
            canvas.Restore();
        }

        private void performCanvasDriftCorrection(Canvas canvas)
        {
            // The width of the view may not be divisible by 7.
            // Therefore, we center everything for half the difference.
            // This will minimize the drift by aligning center correctly
            // and spread the rest of the error towards the start/end.
            var centeringOffset = Width % 7 / 2;
            canvas.Translate(centeringOffset, 0);
        }

        private void drawBackground(Canvas canvas)
        {
            backgroundPaint.Color = backgroundColor;
            canvas.DrawRect(0, 0, Width, Height, backgroundPaint);
        }

        private void drawDay(Canvas canvas, DateTime date)
        {
            textPaint.Color = monthInfo.Month == date.Month
                ? monthTextColor
                : extraMonthTextColor;

            if (monthInfo.Selection?.Contains(date) ?? false)
            {
                drawSelectedDay(canvas, date);
            }
            else if (monthInfo.Today.HasValue && monthInfo.Today == date && !monthInfo.IsTodaySelected)
            {
                drawTodayBackground(canvas);
            }

            drawText(canvas, date.Day.ToString());
        }

        private void drawSelectedDay(Canvas canvas, DateTime date)
        {
            textPaint.Color = selectedTextColor;

            if (monthInfo.IsSelectionPartial)
            {
                textPaint.Color = monthTextColor;
                drawPartialSelection(canvas);
            }
            else if (monthInfo.Selection.Value.IsSingleDay)
            {
                drawSingleDaySelection(canvas);
            }
            else if (monthInfo.IsDateTheFirstSelectedDate(date))
            {
                drawBoundarySelection(canvas, true);
            }
            else if (monthInfo.IsDateTheLastSelectedDate(date))
            {
                drawBoundarySelection(canvas, false);
            }
            else
            {
                drawFullSelection(canvas);
            }
        }

        private void drawTodayBackground(Canvas canvas)
        {
            backgroundPaint.Color = todayMarkerBackgroundColor;
            drawCircle(canvas);
        }

        private void drawPartialSelection(Canvas canvas)
        {
            backgroundPaint.SetStyle(Paint.Style.Stroke);
            backgroundPaint.Color = selectionBackgroundColor;
            drawCircle(canvas);
            backgroundPaint.SetStyle(Paint.Style.Fill);
        }

        private void drawSingleDaySelection(Canvas canvas)
        {
            backgroundPaint.Color = selectionBackgroundColor;
            drawCircle(canvas);
        }

        private void drawFullSelection(Canvas canvas)
        {
            backgroundPaint.Color = selectionBackgroundColor;
            canvas.DrawRect(0, verticalPadding, dayWidth, rowHeight - verticalPadding, backgroundPaint);
        }

        private void drawBoundarySelection(Canvas canvas, bool isLeftBoundary)
        {
            backgroundPaint.Color = selectionBackgroundColor;

            var sign = isLeftBoundary ? -1 : 1;
            var offset = sign * (dayWidth / 2 - radius);
            drawCircle(canvas, offset);

            var xStart = isLeftBoundary ? radius : 0;
            var xEnd = isLeftBoundary ? dayWidth : dayWidth - radius;

            canvas.DrawRect(xStart, verticalPadding, xEnd, rowHeight - verticalPadding, backgroundPaint);
        }

        private void drawCircle(Canvas canvas, int xOffsetFromCenter = 0)
        {
            canvas.DrawCircle(dayWidth / 2 + xOffsetFromCenter, rowHeight / 2, radius, backgroundPaint);
        }

        private void drawText(Canvas canvas, string text)
        {
            textPaint.GetTextBounds(text, 0, text.Length, textBounds);
            var paddingY = (rowHeight - textBounds.Height()) / 2;
            canvas.DrawText(text, dayWidth / 2, rowHeight - paddingY, textPaint);
        }
    }
}
