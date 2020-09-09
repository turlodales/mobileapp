using CoreFoundation;
using CoreGraphics;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Reactive;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.Extensions.Reactive
{
    public static class UIViewExtensions
    {
        public static IObservable<Unit> Tap(this IReactive<UIView> reactive)
            => Observable.Create<Unit>(observer =>
            {
                var gestureRecognizer = new UITapGestureRecognizer(() => observer.OnNext(Unit.Default));
                gestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
                reactive.Base.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        reactive.Base.RemoveGestureRecognizer(gestureRecognizer);
                    });
                });
            });

        public static IObservable<Unit> TapUsingGesture(this IReactive<UIView> reactive)
            => Observable.Create<Unit>(observer =>
            {
                var gestureRecognizer = new UITapGestureRecognizer(_ => observer.OnNext(Unit.Default));
                gestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
                reactive.Base.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        reactive.Base.RemoveGestureRecognizer(gestureRecognizer);
                    });
                });
            });

        public static IObservable<Unit> LongPress(this IReactive<UIView> reactive, bool useFeedback = false)
            => Observable.Create<Unit>(observer =>
            {
                var feedbackGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
                var gestureRecognizer = new UILongPressGestureRecognizer(longPress =>
                {
                    if (longPress.State == UIGestureRecognizerState.Began)
                    {
                        feedbackGenerator.Prepare();
                        observer.OnNext(Unit.Default);

                        if (useFeedback)
                            feedbackGenerator.ImpactOccurred();
                    }
                });
                gestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, otherRecognizer) => true;
                reactive.Base.AddGestureRecognizer(gestureRecognizer);

                return Disposable.Create(() =>
                {
                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        reactive.Base.RemoveGestureRecognizer(gestureRecognizer);
                    });
                });
            });

        public static Action<bool> IsVisible(this IReactive<UIView> reactive)
            => isVisible => reactive.Base.Hidden = !isVisible;

        public static Action<bool> IsVisibleWithFade(this IReactive<UIView> reactive)
            => isVisible =>
            {
                var alpha = isVisible ? 1 : 0;
                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.EaseIn,
                    () =>
                    {
                        reactive.Base.Alpha = alpha;
                    });
            };

        public static Action<UIColor> TintColor(this IReactive<UIView> reactive)
            => color => reactive.Base.TintColor = color;

        public static Action<bool> AnimatedIsVisible(this IReactive<UIView> reactive)
            => isVisible =>
            {
                reactive.Base.Transform = CGAffineTransform.MakeTranslation(0, 20);

                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.SharpCurve,
                    () =>
                    {
                        reactive.Base.Hidden = !isVisible;
                        reactive.Base.Transform = CGAffineTransform.MakeTranslation(0, 0);
                    }
                );
            };

        public static Action<float> AnimatedAlpha(this IReactive<UIView> reactive)
            => alpha =>
            {
                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.SharpCurve,
                    () => reactive.Base.Alpha = alpha
                );
            };

        public static Action<UIColor> BackgroundColor(this IReactive<UIView> reactive) =>
            color => reactive.Base.BackgroundColor = color;

        public static Action<UIColor> AnimatedBackgroundColor(this IReactive<UIView> reactive) =>
            color =>
            {
                AnimationExtensions.Animate(
                    Animation.Timings.EnterTiming,
                    Animation.Curves.SharpCurve,
                    () => reactive.Base.BackgroundColor = color
                );
            };

        public static IDisposable BindAction(this IReactive<UIView> reactive, ViewAction action)
        {
            return Observable.Using(
                    () => action.Enabled.Subscribe(e => { reactive.Base.UserInteractionEnabled = e; }),
                    _ => reactive.Base.Rx().Tap()
                )
                .Subscribe(action.Inputs);
        }

        public static IDisposable BindAction<T>(this IReactive<UIView> reactive, OutputAction<T> action)
        {
            return Observable.Using(
                    () => action.Enabled.Subscribe(e => { reactive.Base.UserInteractionEnabled = e; }),
                    _ => reactive.Base.Rx().Tap()
                )
                .Subscribe(action.Inputs);
        }

        public static IDisposable BindAction<TInput, TElement>(
            this IReactive<UIView> reactive,
            RxAction<TInput, TElement> action,
            Func<UIView, TInput> inputTransform,
            ButtonEventType eventType = ButtonEventType.Tap,
            bool useFeedback = false)
        {
            IObservable<Unit> eventObservable = Observable.Empty<Unit>();
            switch (eventType)
            {
                case ButtonEventType.Tap:
                    eventObservable = reactive.Base.Rx().Tap();
                    break;
                case ButtonEventType.TapGesture:
                    eventObservable = reactive.Base.Rx().TapUsingGesture();
                    break;
                case ButtonEventType.LongPress:
                    eventObservable = reactive.Base.Rx().LongPress(useFeedback);
                    break;
            }

            return Observable.Using(
                    () => action.Enabled.Subscribe(),
                    _ => eventObservable
                )
                .Select(_ => inputTransform(reactive.Base))
                .Subscribe(action.Inputs);
        }

        public static Action<string> AccessibilityLabel(this IReactive<UIView> reactive)
            => accessibilityLabel => reactive.Base.AccessibilityLabel = accessibilityLabel;
    }
}
