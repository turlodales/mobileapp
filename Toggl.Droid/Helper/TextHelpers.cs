using System;
using System.Reactive.Linq;
using Toggl.Core.UI.Extensions;

namespace Toggl.Droid.Helper
{
    public static class TextHelpers
    {
        private const int loadingMessageDotChangeIntervalInMilliseconds = 500;

        private static readonly string[] loadingMessages =
        {
            Shared.Resources.Loading,
            $" {Shared.Resources.Loading}.",
            $"  {Shared.Resources.Loading}..",
            $"   {Shared.Resources.Loading}...",
        };

        public static IObservable<string> AnimatedLoadingMessage()
            => Observable
                .Interval(TimeSpan.FromMilliseconds(loadingMessageDotChangeIntervalInMilliseconds))
                .Select(millis => millis % loadingMessages.Length)
                .Select(dotIndex => loadingMessages[dotIndex])
                .AsDriver(AndroidDependencyContainer.Instance.SchedulerProvider);
    }
}
