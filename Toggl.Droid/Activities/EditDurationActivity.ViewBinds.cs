using Android.Views;
using Android.Widget;
using Toggl.Droid.Extensions;
using Toggl.Droid.Views.EditDuration;

namespace Toggl.Droid.Activities
{
    public partial class EditDurationActivity
    {
        private TextView startLabel;
        private TextView startTimeText;
        private TextView startDateText;
        private TextView stopLabel;
        private TextView stopTimeText;
        private TextView stopTimerLabel;
        private View stopDotSeparator;
        private TextView stopDateText;
        private WheelForegroundView wheelForeground;
        private WheelDurationInput wheelNumericInput;
        private View wheelContainer;
        private TextView durationLabel;

        protected override void InitializeViews()
        {
            startLabel = FindViewById<TextView>(Resource.Id.StartLabel);
            startTimeText = FindViewById<TextView>(Resource.Id.StartTimeText);
            startDateText = FindViewById<TextView>(Resource.Id.StartDateText);
            stopLabel = FindViewById<TextView>(Resource.Id.StopLabel);
            stopTimeText = FindViewById<TextView>(Resource.Id.StopTimeText);
            stopTimerLabel = FindViewById<TextView>(Resource.Id.StopTimerLabel);
            stopDotSeparator = FindViewById<View>(Resource.Id.StopDotSeparator);
            stopDateText = FindViewById<TextView>(Resource.Id.StopDateText);
            wheelForeground = FindViewById<WheelForegroundView>(Resource.Id.WheelForeground);
            wheelNumericInput = FindViewById<WheelDurationInput>(Resource.Id.WheelDurationInput);
            wheelContainer = FindViewById<View>(Resource.Id.Wheel);
            wheelContainer.FitBottomMarginInset();
            durationLabel = FindViewById<TextView>(Resource.Id.DurationLabel);

            startLabel.Text = Shared.Resources.StartTime;
            stopLabel.Text = Shared.Resources.EndTime;
            stopTimerLabel.Text = Shared.Resources.Stop;
            durationLabel.Text = Shared.Resources.Duration;
            
            SetupToolbar(title: Shared.Resources.StartAndStopTime);
        }
    }
}
