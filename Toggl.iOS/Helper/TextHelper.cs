using System;
using System.Reactive.Linq;
using Toggl.Core.UI.Extensions;
using Toggl.Shared;

namespace Toggl.iOS.Helper
{
    public static class TextHelpers
    {
        private const int loadingMessageDotChangeIntervalInMilliseconds = 500;

        private static readonly string[] loadingMessages =
        {
            // these use U+2008, a special ASCII whitespace char, matching the width of 1 period
            // can't use regular spaces since SF is not monospaced
            Resources.Loading,
            $" {Resources.Loading}.",
            $"  {Resources.Loading}..",
            $"   {Resources.Loading}...",
        };

        public static IObservable<string> AnimatedLoadingMessage()
            => Observable
                .Interval(TimeSpan.FromMilliseconds(loadingMessageDotChangeIntervalInMilliseconds))
                .Select(millis => millis % loadingMessages.Length)
                .Select(dotIndex => loadingMessages[dotIndex])
                .AsDriver(IosDependencyContainer.Instance.SchedulerProvider);
    }
}
