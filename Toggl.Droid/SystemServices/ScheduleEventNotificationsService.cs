using System.Reactive.Linq;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Java.Lang;
using Toggl.Droid.Extensions;

namespace Toggl.Droid.SystemServices
{
    [Service(
        Exported = true,
        Permission = "android.permission.BIND_JOB_SERVICE",
        Name = "com.toggl.giskard.ScheduleEventNotificationsService")]
    public class ScheduleEventNotificationsService : JobIntentService
    {
        public const int JobId = 42;

        public static void EnqueueWork(Context context, Intent intent)
        {
            var componentName = new ComponentName(context, JavaUtils.ToClass<ScheduleEventNotificationsService>());
            EnqueueWork(context, componentName, JobId, intent);
        }

        protected override async void OnHandleWork(Intent intent)
        {
            try
            {
                var dependencyContainer = AndroidDependencyContainer.Instance;

                if (!dependencyContainer.UserAccessManager.CheckIfLoggedIn())
                    return;

                var interactorFactory = dependencyContainer.InteractorFactory;

                // Yes, it's ok to run blocking code here, this should run in a background thread as per docs
                // https://developer.android.com/reference/androidx/core/app/JobIntentService#onHandleWork(android.content.Intent)
                await interactorFactory.UpdateEventNotificationsSchedules().Execute();
            }
            catch (SecurityException timedOut)
            {
                //Nothing, if the services times out, we are not doing anything
                //Other exceptions should crash the app/be reported
            }
        }
    }
}
