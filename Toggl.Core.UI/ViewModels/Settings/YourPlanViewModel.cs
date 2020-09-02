using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Xamarin.Essentials;

namespace Toggl.Core.UI.ViewModels
{
    public sealed class YourPlanViewModel : ViewModel
    {
        private readonly IAnalyticsService analyticsService;

        public IObservable<IImmutableList<PlanFeature>> Features { get; }

        public IObservable<string> PlanName { get; }

        public IObservable<string> Expiration { get; }

        public ViewAction OpenTogglWebpage { get; }

        public YourPlanViewModel(
            INavigationService navigationService,
            IInteractorFactory interactorFactory,
            ISchedulerProvider schedulerProvider,
            IAnalyticsService analyticsService,
            IRxActionFactory rxActionFactory) : base(navigationService)
        {
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.analyticsService = analyticsService;

            OpenTogglWebpage = rxActionFactory.FromAsync(openTogglWebpage);

            var planObservable = interactorFactory.ObserveCurrentWorkspacePlan().Execute();

            Features = planObservable.Select(featuresFromPlan).AsDriver(schedulerProvider);
            PlanName = planObservable.Select(plan => plan.Name()).AsDriver(schedulerProvider);
            Expiration = Observable.Return(Resources.NeverExpires).AsDriver(schedulerProvider);
        }

        private Task openTogglWebpage()
        {
            analyticsService.TogglUrlOpenedFromYourWorkspacePlanView.Track();
            return Browser.OpenAsync(Resources.TogglUrl, BrowserLaunchMode.SystemPreferred);
        }

        private IImmutableList<PlanFeature> featuresFromPlan(Plan plan)
            => ImmutableList.Create(
                new PlanFeature(Resources.TrackingTime, true),
                new PlanFeature(Resources.UserGroups, true),
                new PlanFeature(Resources.SummaryAndWeeklyReports, true),
                new PlanFeature(Resources.BillableHours, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.Exporting, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.Rounding, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.SavedReports, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.WorkspaceLogo, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.Estimates, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.Alerts, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.Tasks, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.ProjectDashboard, plan.IsAtLeast(Plan.Premium)),
                new PlanFeature(Resources.IcalFeed, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.CustomProjectColor, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.ProjectTemplate, plan.IsAtLeast(Plan.Starter)),
                new PlanFeature(Resources.MobileAppDesktopAppWebAppTogglButton, true)
            );

        public struct PlanFeature : IEquatable<PlanFeature>
        {
            public string Name { get; }
            public bool Available { get; }

            public PlanFeature(string name, bool available)
            {
                Name = name;
                Available = available;
            }

            public bool Equals(PlanFeature other)
                => Name == other.Name && Available == other.Available;
        }
    }
}

