using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Settings;

namespace Toggl.Storage
{
    public sealed class OnboardingCondition : IDisposable
    {
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();
        private readonly ISubject<Unit> dismissSubject = new Subject<Unit>();

        public OnboardingConditionKey Key { get; }

        public IObservable<bool> ConditionMet { get; }

        public OnboardingCondition(OnboardingConditionKey key, IOnboardingStorage onboardingStorage, IObservable<bool> predicate)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));

            Key = key;

            ConditionMet = onboardingStorage.OnboardingConditionWasMetBefore(Key) || onboardingStorage.CompletedOnboarding()
                ? Observable.Return(false)
                : predicate
                     .Merge(dismissSubject.Select(_ => false))
                     .StartWith(false)
                     .DistinctUntilChanged();

            predicate
                .Where(conditionMet => conditionMet == true)
                .Subscribe(_ => onboardingStorage.SetOnboardingConditionWasMet(Key))
                .DisposedBy(disposeBag);
        }

        public void Dismiss()
            => dismissSubject.OnNext(Unit.Default);

        public void Dispose()
        {
            disposeBag.Dispose();
        }
    }

    public enum OnboardingConditionKey
    {
        RunningTimeEntryTooltip
    }
}
