using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Settings;

namespace Toggl.Storage
{
    public sealed class OnboardingCondition : IDisposable
    {
        private readonly CompositeDisposable disposeBag;

        public string Key { get; }

        public IObservable<bool> ConditionMet { get; }

        public OnboardingCondition(string key, IOnboardingStorage onboardingStorage, IObservable<bool> predicate)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNullOrEmpty(key, nameof(key));

            Key = key;

            ConditionMet = onboardingStorage.OnboardingConditionWasMetBefore(this)
                ? Observable.Return(false)
                : predicate;

            predicate
                .Where(conditionMet => conditionMet == true)
                .Subscribe(_ => onboardingStorage.SetOnboardingConditionWasMet(this))
                .DisposedBy(disposeBag);
        }

        public void Dispose()
        {
            disposeBag.Dispose();
        }
    }
}
