using System;
using System.Reactive;
using Toggl.Core.Analytics;
using Toggl.Storage;

namespace Toggl.Core.UI.Extensions
{
    public static class OnboardingExtensions
    {
        public static TrackingOnboardingCondition TrackingDismissEvents(
            this OnboardingCondition onboardingCondition,
            IAnalyticsService analyticsService)
            => new TrackingOnboardingCondition(onboardingCondition, analyticsService);
    }

    public class TrackingOnboardingCondition
    {
        private readonly OnboardingCondition onboardingCondition;
        private readonly IAnalyticsService analyticsService;

        public OnboardingConditionKey Key => onboardingCondition.Key;

        public IObservable<bool> ConditionMet => onboardingCondition.ConditionMet;

        public IObservable<Unit> Dismissed => onboardingCondition.Dismissed;

        public TrackingOnboardingCondition(OnboardingCondition onboardingCondition, IAnalyticsService analyticsService)
        {
            this.onboardingCondition = onboardingCondition;
            this.analyticsService = analyticsService;
        }

        public void Dismiss()
        {
            analyticsService.TooltipDismissed.Track(Key, TooltipDismissReason.ManuallyDismissed);
            onboardingCondition.Dismiss();
        }
    }
}
