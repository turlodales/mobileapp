using Android.App;
using Android.Content;
using AndroidX.Work;
using Toggl.Droid.Helper;
using Toggl.Droid.Services;
using Toggl.Droid.SystemServices;
using Toggl.Droid.Widgets;

namespace Toggl.Droid.BroadcastReceivers
{
    [BroadcastReceiver(
        Exported = true,
        Permission = "android.permission.BIND_JOB_SERVICE",
        Name = "com.toggl.giskard.BootCompletedBroadcastReceiver")]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.CategoryDefault })]
    public class BootCompletedBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var scheduleNotificationsRequest = new OneTimeWorkRequest.Builder(typeof(ScheduleEventNotificationsWorker)).Build();
            WorkManager.GetInstance(context).Enqueue(scheduleNotificationsRequest);
            AppWidgetProviderUtils.UpdateAllInstances<TimeEntryWidget>();
            AppWidgetProviderUtils.UpdateAllInstances<SuggestionsWidget>();
        }
    }
}
