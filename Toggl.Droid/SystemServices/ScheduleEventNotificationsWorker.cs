using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.Work;

namespace Toggl.Droid.SystemServices
{
    public class ScheduleEventNotificationsWorker : Worker
    {
        public ScheduleEventNotificationsWorker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ScheduleEventNotificationsWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            try
            {
                var dependencyContainer = AndroidDependencyContainer.Instance;

                if (!dependencyContainer.UserAccessManager.CheckIfLoggedIn())
                {
                    return Result.InvokeSuccess();
                }

                var interactorFactory = dependencyContainer.InteractorFactory;
                interactorFactory.UpdateEventNotificationsSchedules().Execute().GetAwaiter().GetResult();
                return Result.InvokeSuccess();
            }
            catch (Exception exception)
            {
                return Result.InvokeFailure();
            }
        }
    }
}