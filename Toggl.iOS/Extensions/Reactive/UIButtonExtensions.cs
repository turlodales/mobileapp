using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Reactive;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.Extensions.Reactive
{
    public enum ButtonEventType
    {
        Tap,
        LongPress
    }

    public static class UIButtonExtensions
    {
        public static IObservable<Unit> Tap(this IReactive<UIButton> reactive)
            => Observable
                .FromEventPattern(e => reactive.Base.TouchUpInside += e, e => reactive.Base.TouchUpInside -= e)
                .SelectUnit();

        public static Action<string> Title(this IReactive<UIButton> reactive)
            => title =>
            {
                UIView.PerformWithoutAnimation(() =>
                {
                    reactive.Base.SetTitle(title, UIControlState.Normal);
                    reactive.Base.LayoutIfNeeded();
                });
            };

        public static Action<string> TitleAdaptive(this IReactive<UIButton> reactive)
            => title =>
            {
                reactive.Base.SetTitle(title, UIControlState.Normal);
                reactive.Base.SizeToFit();
            };

        public static Action<UIColor> TitleColor(this IReactive<UIButton> reactive)
            => color => reactive.Base.SetTitleColor(color, UIControlState.Normal);

        public static Action<string> AnimatedTitle(this IReactive<UIButton> reactive)
            => title =>
            {
                UIView.Transition(
                    reactive.Base,
                    Animation.Timings.EnterTiming,
                    UIViewAnimationOptions.TransitionCrossDissolve,
                    () => reactive.Base.SetTitle(title, UIControlState.Normal),
                    null
                );
            };

        public static IDisposable BindAction(this IReactive<UIButton> reactive, ViewAction action) =>
            reactive.BindAction(action, _ => Unit.Default);

        public static IDisposable BindAction<TInput, TElement>(this IReactive<UIButton> reactive,
            RxAction<TInput, TElement> action, Func<UIButton, TInput> inputTransform)
        {
            return Observable.Using(
                    () => action.Enabled.Subscribe(e => { reactive.Base.Enabled = e; }),
                    _ => reactive.Base.Rx().Tap()
                )
                .Select(_ => inputTransform(reactive.Base))
                .Subscribe(action.Inputs);
        }
    }
}
