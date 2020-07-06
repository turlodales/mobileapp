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
    public sealed class OnboardingCondition
    {
        private readonly ISubject<Unit> dismissSubject = new Subject<Unit>();

        private readonly IOnboardingStorage onboardingStorage;

        public OnboardingConditionKey Key { get; }

        public IObservable<bool> ConditionMet { get; }

        public OnboardingCondition(
        OnboardingConditionKey key,
        IOnboardingStorage onboardingStorage,
        ISchedulerProvider schedulerProvider,
        IObservable<bool> predicate)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));

            this.onboardingStorage = onboardingStorage;

            Key = key;

            ConditionMet = onboardingStorage.OnboardingConditionWasMetBefore(Key) ||
                           onboardingStorage.CompletedOnboarding()
                ? Observable.Return(false)
                : predicate
                    .Merge(dismissSubject.Select(_ => false))
                    .StartWith(false)
                    .DistinctUntilChanged()
                    .Select(conditionWasNotMetBefore)
                    .Do(setConditionWasMetBeforeIfNeeded)
                    .ObserveOn(schedulerProvider.MainScheduler);
        }

        private bool conditionWasNotMetBefore(bool shouldShow)
        {
            if (onboardingStorage.OnboardingConditionWasMetBefore(Key) || onboardingStorage.CompletedOnboarding())
                return false;
            return shouldShow;
        }

        private void setConditionWasMetBeforeIfNeeded(bool shouldShow)
        {
            if (shouldShow)
                onboardingStorage.SetOnboardingConditionWasMet(Key);
        }

        public void Dismiss()
            => dismissSubject.OnNext(Unit.Default);
    }

    public enum OnboardingConditionKey
    {
        RunningTimeEntryTooltip,
        StartTimeEntryTooltip,
        TapToStopTimeEntryTooltip,
        StartViewProjectsTooltip,
        EditViewProjectsTooltip,
        FinalTooltip
    }
}
