using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Immutable;
using System.Linq;
using Toggl.Droid.Extensions;
using Toggl.Droid.ViewHelpers;
using Toggl.Shared;
using Color = Android.Graphics.Color;
using static Toggl.Core.UI.ViewModels.Reports.ReportBarChartElement;
using Math = System.Math;

namespace Toggl.Droid.Views
{
    [Register("toggl.droid.views.BarChartView")]
    public class BarChartView : View
    {
        private const float barDrawingYTranslationAdjustmentInPixels = 1f;
        private const float defaultBarSpacingRatio = 0.2f;
        private const float minHeightForBarsWithPercentages = 1f;

        private readonly Paint filledValuePaint = new Paint();
        private readonly Paint regularValuePaint = new Paint();
        private readonly Paint filledValuePlaceholderPaint = new Paint();
        private readonly Paint regularValuePlaceholderPaint = new Paint();
        private readonly Paint othersPaint = new Paint();

        private readonly Rect bounds = new Rect();

        private float maxWidth;
        private float barsRightMargin;
        private float barsLeftMargin;
        private float barsBottomMargin;
        private float barsTopMargin;
        private float textSize;
        private float textLeftMargin;
        private float textBottomMargin;
        private float bottomLabelMarginTop;
        private float dateTopPadding;

        private int barsCount;
        private bool willDrawBarChart;
        private float barsWidth;
        private float barsHeight;
        private float actualBarWidth;
        private int spaces;
        private float totalWidth;
        private float remainingWidth;
        private float spacing;
        private float requiredWidth;
        private float middleHorizontalLineY;
        private float barsBottom;
        private float hoursLabelsX;
        private float hoursBottomMargin;
        private bool willDrawIndividualLabels;
        private float startEndDatesY;
        private float barsStartingLeft;
        private bool shouldDrawEmptyState;

        private IImmutableList<Bar> bars;
        private IImmutableList<string> xLabels;
        private YAxisLabels yLabels;
        private double maxValue;
        private string startDate;
        private string endDate;
        private float dayLabelsY;
        private IImmutableList<Bar> placeholderBars = generatePlaceholderBars();

        private static YAxisLabels placeholderYLabels = new YAxisLabels("10 h", "5 h", "0 h");
        private static IImmutableList<string> placeholderXLabels = new string[] { "", "" }.ToImmutableList();
        private bool isDrawingPlaceholders() => bars == placeholderBars;

        public BarChartView(Context context) : base(context)
        {
            initialize(context);
        }

        public BarChartView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            initialize(context);

            var customAttrs = Context.ObtainStyledAttributes(attrs, Resource.Styleable.BarChartView, 0, 0);
            try
            {
                shouldDrawEmptyState = customAttrs.GetBoolean(Resource.Styleable.BarChartView_drawEmptyState, false);
                if (shouldDrawEmptyState)
                {
                    bars = placeholderBars = generateEmptyStateBars();
                    xLabels = new[] {"01/04", "30/04"}.ToImmutableList();
                    yLabels = YAxisLabels.Empty;
                    updateBarChartDrawingData();
                }
            }
            finally
            {
                customAttrs.Recycle();
            }
        }

        protected BarChartView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        private Color horizontalLineColor;
        private Color hoursTextColor;
        private Color emptyBarColor;
        private Color xAxisLegendColor;

        private void initialize(Context context)
        {
            maxWidth = 48.DpToPixels(context);
            barsRightMargin = 40.DpToPixels(context);
            barsLeftMargin = 12.DpToPixels(context);
            barsBottomMargin = 40.DpToPixels(context);
            barsTopMargin = 18.DpToPixels(context);
            textSize = 12.SpToPixels(context);
            textLeftMargin = 12.DpToPixels(context);
            textBottomMargin = 4.DpToPixels(context);
            bottomLabelMarginTop = 12.DpToPixels(context);
            dateTopPadding = 4.DpToPixels(context);

            othersPaint.TextSize = textSize;
            filledValuePaint.Color = context.SafeGetColor(Resource.Color.filledChartBar);
            regularValuePaint.Color = context.SafeGetColor(Resource.Color.regularChartBar);
            filledValuePlaceholderPaint.Color = context.SafeGetColor(Resource.Color.placeholderBarFilled);
            regularValuePlaceholderPaint.Color = context.SafeGetColor(Resource.Color.placeholderBarRegular);
            regularValuePaint.SetStyle(Paint.Style.FillAndStroke);
            filledValuePaint.SetStyle(Paint.Style.FillAndStroke);
            regularValuePlaceholderPaint.SetStyle(Paint.Style.FillAndStroke);
            filledValuePlaceholderPaint.SetStyle(Paint.Style.FillAndStroke);

            emptyBarColor = context.SafeGetColor(Resource.Color.placeholderText);
            horizontalLineColor = context.SafeGetColor(Resource.Color.separator);
            hoursTextColor = context.SafeGetColor(Resource.Color.placeholderText);
            xAxisLegendColor = context.SafeGetColor(Resource.Color.secondaryText);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            updateBarChartDrawingData();
            PostInvalidate();
        }

        public void updateBars(IImmutableList<Bar> newBars, double maxValue, IImmutableList<string> xLabels, YAxisLabels yLabels)
        {
            this.maxValue = maxValue;
            bars = newBars;
            this.yLabels = yLabels;
            this.xLabels = xLabels;

            updateBarChartDrawingData();
            PostInvalidate();
        }

        private void updateBarChartDrawingData()
        {
            if (shouldDrawEmptyState)
            {
                barsRightMargin = barsLeftMargin;
            }

            if (bars == null)
            {
                willDrawBarChart = false;
                return;
            }

            barsCount = bars.Count;
            willDrawBarChart = barsCount > 0;

            if (!willDrawBarChart) return;

            startDate = xLabels[0];
            endDate = xLabels[xLabels.Count - 1];

            barsWidth = MeasuredWidth - barsLeftMargin - barsRightMargin;
            barsHeight = MeasuredHeight - barsTopMargin - barsBottomMargin;

            var idealBarWidth = Math.Min(barsWidth / barsCount, maxWidth);
            spaces = barsCount - 1;
            totalWidth = idealBarWidth * barsCount;
            remainingWidth = barsWidth - totalWidth;
            spacing = Math.Max(remainingWidth / barsCount, idealBarWidth * defaultBarSpacingRatio);
            requiredWidth = totalWidth + spaces * spacing;
            actualBarWidth = requiredWidth > barsWidth ? idealBarWidth * (1 - defaultBarSpacingRatio) : idealBarWidth;
            middleHorizontalLineY = barsHeight / 2f + barsTopMargin;
            barsBottom = MeasuredHeight - barsBottomMargin;

            hoursLabelsX = MeasuredWidth - barsRightMargin + textLeftMargin;
            hoursBottomMargin = textBottomMargin * 2f;

            willDrawIndividualLabels = xLabels.Count > 2;
            startEndDatesY = barsBottom + bottomLabelMarginTop * 2f;
            dayLabelsY = barsBottom + bottomLabelMarginTop * 1.25f;
            barsStartingLeft = barsLeftMargin + (barsWidth - (actualBarWidth * barsCount + spaces * spacing)) / 2f;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (!willDrawBarChart)
            {
                bars = placeholderBars;
                xLabels = placeholderXLabels;
                yLabels = placeholderYLabels;
                maxValue = placeholderBars.Max(bar => bar.TotalValue);
                updateBarChartDrawingData();
            }

            drawHorizontalLines(canvas);
            drawYAxisLegend(canvas);
            drawXAxisLegendIfNotDrawingIndividualLabels(canvas);
            drawBarsAndXAxisLegend(canvas);
        }

        private void drawHorizontalLines(Canvas canvas)
        {
            if (shouldDrawEmptyState)
            {
                //We do need the bottom one though
                canvas.DrawLine(0f, barsBottom, Width, barsBottom, othersPaint);
                return;
            }

            othersPaint.Color = horizontalLineColor;
            canvas.DrawLine(0f, barsTopMargin, Width, barsTopMargin, othersPaint);
            canvas.DrawLine(0f, middleHorizontalLineY, Width, middleHorizontalLineY, othersPaint);
            canvas.DrawLine(0f, barsBottom, Width, barsBottom, othersPaint);
        }

        private void drawYAxisLegend(Canvas canvas)
        {
            othersPaint.Color = hoursTextColor;
            canvas.DrawText(yLabels.BottomLabel, hoursLabelsX, barsBottom - hoursBottomMargin, othersPaint);
            canvas.DrawText(yLabels.MiddleLabel, hoursLabelsX, middleHorizontalLineY - hoursBottomMargin, othersPaint);
            canvas.DrawText(yLabels.TopLabel, hoursLabelsX, barsTopMargin - hoursBottomMargin, othersPaint);
        }

        private void drawXAxisLegendIfNotDrawingIndividualLabels(Canvas canvas)
        {
            if (!willDrawIndividualLabels && (!isDrawingPlaceholders() || shouldDrawEmptyState))
            {
                othersPaint.Color = xAxisLegendColor;
                othersPaint.TextAlign = Paint.Align.Left;
                canvas.DrawText(startDate, barsLeftMargin, startEndDatesY, othersPaint);
                othersPaint.TextAlign = Paint.Align.Right;
                canvas.DrawText(endDate, Width - barsRightMargin, startEndDatesY, othersPaint);
            }
        }

        private void drawBarsAndXAxisLegend(Canvas canvas)
        {
            var left = barsStartingLeft;

            othersPaint.TextAlign = Paint.Align.Center;
            var originalTextSize = othersPaint.TextSize;

            var barsToRender = bars;
            var labelsToRender = xLabels;
            var numberOfLabels = xLabels.Count;
            var filledPaintToUse = isDrawingPlaceholders() ? filledValuePlaceholderPaint : filledValuePaint;
            var regularPaintToUse = isDrawingPlaceholders() ? regularValuePlaceholderPaint : regularValuePaint;

            for (var i = 0; i < barsToRender.Count; i++)
            {
                var bar = barsToRender[i];
                var barRight = left + actualBarWidth;
                var barHasFilledPercentage = bar.FilledValue > 0;
                var barHasTransparentPercentage = bar.TotalValue > bar.FilledValue;
                if (!barHasFilledPercentage && !barHasTransparentPercentage)
                {
                    othersPaint.Color = emptyBarColor;
                    canvas.DrawLine(left, barsBottom, barRight, barsBottom, othersPaint);
                }
                else
                {
                    var filledBarHeight = (float)(barsHeight * bar.FilledValue);
                    var filledTop = calculateFilledTop(filledBarHeight, barHasFilledPercentage);
                    canvas.DrawRect(left, filledTop, barRight, barsBottom + barDrawingYTranslationAdjustmentInPixels, filledPaintToUse);

                    var regularBarHeight = (float)(barsHeight * (bar.TotalValue - bar.FilledValue));
                    var regularTop = calculateRegularTop(filledTop, regularBarHeight, barHasTransparentPercentage);
                    canvas.DrawRect(left, regularTop, barRight, filledTop, regularPaintToUse);
                }

                if (willDrawIndividualLabels && i < numberOfLabels)
                {
                    var horizontalLabelElements = labelsToRender[i].Split("\n");
                    if (horizontalLabelElements.Length == 2)
                    {
                        othersPaint.Color = xAxisLegendColor;
                        var middleOfTheBar = left + (barRight - left) / 2f;
                        var dayOfWeekText = horizontalLabelElements[1];
                        othersPaint.TextSize = originalTextSize;
                        canvas.DrawText(dayOfWeekText, middleOfTheBar, dayLabelsY, othersPaint);

                        var dateText = horizontalLabelElements[0];
                        othersPaint.UpdatePaintForTextToFitWidth(dateText, actualBarWidth, bounds);
                        othersPaint.GetTextBounds(dateText, 0, dateText.Length, bounds);
                        canvas.DrawText(dateText, middleOfTheBar, dayLabelsY + bounds.Height() + dateTopPadding, othersPaint);
                    }
                }

                left += actualBarWidth + spacing;
            }
        }

        private float calculateFilledTop(float filledBarHeight, bool barHasFilledPercentage)
        {
            var filledTop = barsBottom - filledBarHeight + barDrawingYTranslationAdjustmentInPixels;
            var barHasAtLeast1PixelInHeight = filledBarHeight >= minHeightForBarsWithPercentages;

            return barHasFilledPercentage && !barHasAtLeast1PixelInHeight
                ? filledTop - minHeightForBarsWithPercentages
                : filledTop;
        }

        private float calculateRegularTop(float filledTop, float regularBarHeight, bool barHasRegularPercentage)
        {
            //Regular top doesn't need the extra Y translation because the filledTop accounts for it.
            var regularTop = filledTop - regularBarHeight;
            var barHasAtLeast1PixelInHeight = regularBarHeight >= minHeightForBarsWithPercentages;

            return barHasRegularPercentage && !barHasAtLeast1PixelInHeight
                ? regularTop - minHeightForBarsWithPercentages
                : regularTop;
        }

        private static IImmutableList<Bar> generatePlaceholderBars()
        {
            var maxTotal = 78.2 + 67.3;
            return new (double Filled, double Regular)[] {
                (12.4, 6.9),
                (7.6, 13.5),
                (12.4, 6.9),
                (15.1, 18.9),
                (15.1, 20.1),
                (14.6, 24.5),
                (9.5, 28.2),
                (11.3, 23.9),
                (11.5, 32.9),
                (10.5, 22.8),
                (35.5, 24.5),
                (36.1, 24.2),
                (41.5, 31.2),
                (40, 33.3),
                (8.3, 43),
                (39.9, 35.6),
                (10, 50.9),
                (14.2, 46.7),
                (39.7, 50.2),
                (40.8, 44),
                (7.6, 43.9),
                (56.1, 55.7),
                (52.7, 53.2),
                (66.5, 50.7),
                (53.3, 54.6),
                (59, 64),
                (73, 61),
                (71.1, 70),
                (73.8, 72),
                (64.5, 79.8),
                (78.2, 67.3),
            }
            .Select(tuple => new Bar(tuple.Filled / maxTotal, (tuple.Filled + tuple.Regular) / maxTotal))
            .ToImmutableList();
        }

        private static IImmutableList<Bar> generateEmptyStateBars()
            => new[]
                {
                    3, 10, 3, 9, 10, 9, 14, 8, 11, 13, 14, 16, 20, 20, 41, 44, 37, 21, 39, 43, 32, 39, 31, 28, 36, 36,
                    41, 38, 37, 38
                }
                .Select(height => new Bar( height, height).Scaled(100))
                .ToImmutableList();
    }
}
