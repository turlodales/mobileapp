using System;
using Android.Content;
using Android.Runtime;
using AndroidX.Work;
using Exception = System.Exception;

namespace Toggl.Droid.SystemServices
{
    public sealed class WidgetsAnalyticsWorker : Worker
    {
        public const string WidgetsAnalyticsWorkerAction = nameof(WidgetsAnalyticsWorkerAction);
        
        public const string TimerWidgetInstallAction = nameof(TimerWidgetInstallAction);
        public const string TimerWidgetInstallStateParameter = nameof(TimerWidgetInstallStateParameter);

        public const string TimerWidgetResizeAction = nameof(TimerWidgetResizeAction);
        public const string TimerWidgetSizeParameter = nameof(TimerWidgetSizeParameter);

        public const string SuggestionsWidgetInstallAction = nameof(SuggestionsWidgetInstallAction);
        public const string SuggestionsWidgetInstallStateParameter = nameof(SuggestionsWidgetInstallStateParameter);

        public WidgetsAnalyticsWorker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WidgetsAnalyticsWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public static void EnqueueWork(Context context, Data data)
        {
            var widgetAnalyticsWorker = new OneTimeWorkRequest.Builder(typeof(WidgetsAnalyticsWorker))
                .SetInputData(data)
                .Build();
            
            WorkManager.GetInstance(context).Enqueue(widgetAnalyticsWorker);
        }

        private void handleTrackTimerWidgetInstallState()
        {
            var installationState = InputData.GetBoolean(TimerWidgetInstallStateParameter, false);
            var analyticsService = AndroidDependencyContainer.Instance.AnalyticsService;
            analyticsService.TimerWidgetInstallStateChange.Track(installationState);
        }

        private void handleTrackTimerWidgetResize()
        {
            var widgetSize = InputData.GetInt(TimerWidgetSizeParameter, 1);
            var analyticsService = AndroidDependencyContainer.Instance.AnalyticsService;
            analyticsService.TimerWidgetSizeChanged.Track(widgetSize);
        }

        private void handleTrackSuggestionsWidgetInstallState()
        {
            var installationState = InputData.GetBoolean(SuggestionsWidgetInstallStateParameter, false);
            var analyticsService = AndroidDependencyContainer.Instance.AnalyticsService;
            analyticsService.SuggestionsWidgetInstallStateChange.Track(installationState);
        }

        public override Result DoWork()
        {
            try
            {
                var action = InputData.GetString(WidgetsAnalyticsWorkerAction);
                switch (action)
                {
                    case TimerWidgetInstallAction:
                        handleTrackTimerWidgetInstallState();
                        break;
                    case TimerWidgetResizeAction:
                        handleTrackTimerWidgetResize();
                        break;
                    case SuggestionsWidgetInstallAction:
                        handleTrackSuggestionsWidgetInstallState();
                        break;
                }
                return Result.InvokeSuccess();
            }
            catch (Exception exception)
            {
                return Result.InvokeFailure();
            }
        }
    }
}
