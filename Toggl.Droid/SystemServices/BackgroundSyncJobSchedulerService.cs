using System;
using Android.App;
using Android.App.Job;
using Toggl.Droid.Helper;
using static Toggl.Shared.Extensions.CommonFunctions;

namespace Toggl.Droid.SystemServices
{
    [Service(Exported = true,
             Permission = "android.permission.BIND_JOB_SERVICE",
             Name = "com.toggl.giskard.BackgroundSyncJobSchedulerService")]
    public class BackgroundSyncJobSchedulerService : JobService
    {
        public const int JobId = 1;

        private IDisposable disposable;

        public override bool OnStartJob(JobParameters @params)
        {
            AndroidDependencyContainer.EnsureInitialized(ApplicationContext);
            var dependencyContainer = AndroidDependencyContainer.Instance;
            if (!dependencyContainer.UserAccessManager.CheckIfLoggedIn())
                return false;

            // The widgets service will listen for changes to the running
            // time entry and it will update the data in the shared database
            // and that way the widget will show correct information after we sync.
            dependencyContainer.WidgetsService.Start();

            disposable = dependencyContainer.InteractorFactory.RunBackgroundSync()
                .Execute()
                .Subscribe(DoNothing, DoNothing,
                    () => JobFinished(@params, false));
            
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            AndroidDependencyContainer
                .Instance.AnalyticsService
                .BackgroundSyncMustStopExcecution.Track();

            disposable?.Dispose();
            return true;
        }
    }
}
