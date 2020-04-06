using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Immutable;
using System.Linq;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Extensions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Point = Toggl.Shared.Point;
using Color = Android.Graphics.Color;

namespace Toggl.Droid.Views
{
    [Register("toggl.droid.views.DonutChartView")]
    public sealed partial class DonutChartView : View
    {
        private const float fullCircle = 360f;
        private const float innerCircleFactor = 0.6f;
        private const float selectionMarkerMinRadius = 0.54f;
        private const float selectionMarkerMaxRadius = 0.58f;
        private const float angleRightToTopCorrection = 270;
        private const float minimumSegmentPercentageToShowLabel = 0.04f;
        private const float sliceLabelCenterRadius = (1 + innerCircleFactor) / 2;
        private const int maxLabelLengthAtMinSize = 16;

        private Paint paint = new Paint() { AntiAlias = true };
        private Paint textPaint = new Paint() { AntiAlias = true, TextAlign = Paint.Align.Center };
        private Rect bounds = new Rect();
        private RectF circle = new RectF();

        private Color backgroundColor;
        private Color donutColor;
        private Color donutSelectionDurationTextColor;

        private Point center;
        private float halfWidth;
        private float innerCircleRadius;
        private float selectionCircleRadius;
        private float selectedSliceLabelVerticalOffset;
        private float selectedSliceValueVerticalOffset;
        private float selectedSliceLabelTextSize;

        private int percentageLabelTextSize;
        private int selectedSliceDurationTextSize;

        private bool isLoading;
        private SlicesCollection slices = new SlicesCollection(ImmutableList<ReportDonutChartElement.PercentageDecoratedSegment>.Empty);
        private Slice selectedSlice;

        private SliceSelectionAnimator selectionAnimator;

        private Func<double, string> valueFormatter = defaultValueFormatter;
        private static string defaultValueFormatter(double value) => value.ToString();

        public DonutChartView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public DonutChartView(Context context)
            : base(context)
        {
            initialize(context);
        }

        public DonutChartView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            initialize(context);
        }
        private void initialize(Context context)
        {
            backgroundColor = context.SafeGetColor(Resource.Color.cardBackground);
            donutColor = context.SafeGetColor(Resource.Color.placeholderDonut);
            donutSelectionDurationTextColor = context.SafeGetColor(Resource.Color.donutSelectionDurationText);

            percentageLabelTextSize = 11.SpToPixels(context);
            selectedSliceDurationTextSize = 16.SpToPixels(context);
            selectedSliceLabelTextSize = 16.SpToPixels(context);
            selectionAnimator = new SliceSelectionAnimator(PostInvalidate);

            selectedSliceLabelVerticalOffset = 12.DpToPixels(context);
            selectedSliceValueVerticalOffset = -12.DpToPixels(context);
        }

        public void Update(ReportDonutChartDonutElement element)
        {
            isLoading = element.IsLoading;

            var percentageSegments = DonutChartGrouping.Group(element.Segments);

            slices = new SlicesCollection(percentageSegments);
            selectedSlice = null;

            valueFormatter = element.ValueFormatter ?? defaultValueFormatter;

            PostInvalidate();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, widthMeasureSpec);

            var width = MeasureSpec.GetSize(widthMeasureSpec);

            halfWidth = width / 2;

            center = new Point(halfWidth, halfWidth);

            innerCircleRadius = width * innerCircleFactor / 2;
            selectionCircleRadius = width * selectionMarkerMinRadius / 2;
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (slices == null)
                return;

            if (isLoading)
            {
                drawIsLoading(canvas);
                return;
            }

            canvas.Save();
            canvas.Rotate(angleRightToTopCorrection, (float)center.X, (float)center.Y);

            drawSlices(canvas);

            drawSelectedSlice(canvas);

            canvas.Restore();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (e == null || !slices.Any())
                return false;

            if (e.Action != MotionEventActions.Up)
                return base.OnTouchEvent(e);

            // Location of the view from top left corner of the SCREEN
            var viewLocation = this.GetLocationOnScreen();

            // Location of the touch from top left corner of the SCREEN
            var rawLocation = new Point(e.RawX, e.RawY);

            // Location of the touch from top left corner of the VIEW
            var touchLocationInView = rawLocation - viewLocation;

            // Location of the touch from the CENTER of the VIEW
            var touchLocationFromViewCenter = touchLocationInView - center;

            var angle = (float)touchLocationFromViewCenter.AngleInDegrees - angleRightToTopCorrection;
            angle = angle.NormalizedAngle();

            var normalizedDistance = touchLocationFromViewCenter.Magnitude / halfWidth;

            if (normalizedDistance < innerCircleFactor)
            {
                selectedSlice = null;
            }
            else
            {
                var slice = slices.GetSliceForPercentageValue(angle);

                var color = Color.ParseColor(slice.PercentageSegment.Segment.Color);

                if (selectedSlice == null)
                {
                    // nothing was selected before, work from the middle of the newly selected slice
                    var midAngle = slice.StartAngle + slice.SweepAngle / 2;

                    selectionAnimator.StartAnimation(slice.StartAngle, slice.SweepAngle, color, midAngle, 0);
                }
                else if (slice != selectedSlice)
                {
                    // something is already selected, continue from there
                    // (or from the current animation state, for mid-animation taps)
                    selectionAnimator.ContinueAnimation(slice.StartAngle, slice.SweepAngle, color);
                }

                selectedSlice = slice;
            }

            PostInvalidate();
            return false;
        }

        private void drawIsLoading(Canvas canvas)
        {
            paint.Color = donutColor;

            canvas.DrawCircle((float)center.X, (float)center.X, Width / 2, paint);

            drawInnerCircle(canvas);
        }

        private void drawSlices(Canvas canvas)
        {
            circle.Set(0, 0, Width, Height);

            foreach (var slice in slices)
            {
                drawSlice(canvas, circle, slice);
            }

            drawInnerCircle(canvas);
        }

        private void drawSlice(Canvas canvas, RectF circle, Slice slice)
        {
            paint.Color = Color.ParseColor(slice.PercentageSegment.Segment.Color);
            canvas.DrawArc(circle, slice.StartAngle, slice.SweepAngle, true, paint);

            drawSegmentPercentageLabel(canvas, slice);
        }

        private void drawSelectedSlice(Canvas canvas)
        {
            if (selectedSlice == null)
                return;

            drawSelectionMarker(canvas);
            drawSelectedSliceLabels(canvas);
        }

        private void drawSelectionMarker(Canvas canvas)
        {
            var circleWidth = Width * selectionMarkerMaxRadius;
            var circleHeight = Height * selectionMarkerMaxRadius;
            var offsetX = (Width - circleWidth) / 2;
            var offsetY = (Height - circleWidth) / 2;

            circle.Set(offsetX, offsetY, offsetX + circleWidth, offsetY + circleHeight);

            paint.Color = selectionAnimator.Color;
            canvas.DrawArc(circle, selectionAnimator.StartAngle, selectionAnimator.SweepAngle, true, paint);

            drawCircle(canvas, selectionCircleRadius);
        }

        private void drawSelectedSliceLabels(Canvas canvas)
        {
            var label = selectedSlice.PercentageSegment.Segment.Label;
            textPaint.TextSize = selectedSliceLabelTextSize;
            label = label.Ellipsize(maxLabelLengthAtMinSize);

            textPaint.Color = selectionAnimator.Color;
            var labelPosition = center + new Point(0, selectedSliceLabelVerticalOffset);
            drawLabel(canvas, label, labelPosition);

            textPaint.Color = donutSelectionDurationTextColor;
            var valuePosition = center + new Point(0, selectedSliceValueVerticalOffset);
            var formattedValue = valueFormatter(selectedSlice.PercentageSegment.Segment.Value);
            textPaint.TextSize = selectedSliceDurationTextSize;
            drawLabel(canvas, formattedValue, valuePosition);
        }

        private void drawInnerCircle(Canvas canvas)
            => drawCircle(canvas, innerCircleRadius);

        private void drawCircle(Canvas canvas, float radius, Color? color = null)
        {
            paint.Color = color ?? backgroundColor;
            canvas.DrawCircle((float)center.X, (float)center.Y, radius, paint);
        }

        private void drawSegmentPercentageLabel(Canvas canvas, Slice slice)
        {
            if (slice.Percentage < minimumSegmentPercentageToShowLabel)
                return;

            var labelAngle = (angleRightToTopCorrection + slice.StartAngle + slice.SweepAngle / 2).ToRadians();

            var x = center.X * (1 + sliceLabelCenterRadius * MathF.Cos(labelAngle));
            var y = center.Y * (1 + sliceLabelCenterRadius * MathF.Sin(labelAngle));

            textPaint.TextSize = percentageLabelTextSize;
            textPaint.Color = Color.White;
            drawLabel(canvas, $"{(int)(100 * slice.OriginalPercentage)}%", new Point(x, y));
        }

        private void drawLabel(Canvas canvas, string text, Point textCenter)
        {
            canvas.Save();
            canvas.Rotate(-angleRightToTopCorrection, (float)center.X, (float)center.Y);

            textPaint.GetTextBounds(text, 0, text.Length, bounds);

            var offsetY = bounds.Height() / 2;

            canvas.DrawText(text, (float)textCenter.X, (float)(textCenter.Y + offsetY), textPaint);

            canvas.Restore();
        }
    }
}
