using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainTabBarViewModel : ViewModelWithInput<MainTabBarParameters>
    {
        public Lazy<ViewModel> MainViewModel { get; }
        public Lazy<ViewModel> ReportsViewModel { get; }
        public Lazy<ViewModel> CalendarViewModel { get; }

        public IImmutableList<Lazy<ViewModel>> Tabs { get; }

        public IObservable<MainTabBarParameters.SsoLinkResult> SsoLinkResult;
        private BehaviorSubject<MainTabBarParameters.SsoLinkResult> ssoLinkResultSubject = new BehaviorSubject<MainTabBarParameters.SsoLinkResult>(MainTabBarParameters.SsoLinkResult.NONE);

        public MainTabBarViewModel(UIDependencyContainer container)
            : base(container.NavigationService)
        {
            Ensure.Argument.IsNotNull(container, nameof(container));
            Ensure.Argument.IsNotNull(container.SchedulerProvider, nameof(container.SchedulerProvider));

            MainViewModel = new Lazy<ViewModel>(() => new MainViewModel(
                container.DataSource,
                container.SyncManager,
                container.TimeService,
                container.RatingService,
                container.UserPreferences,
                container.AnalyticsService,
                container.OnboardingStorage,
                container.InteractorFactory,
                container.NavigationService,
                container.RemoteConfigService,
                container.AccessibilityService,
                container.UpdateRemoteConfigCacheService,
                container.AccessRestrictionStorage,
                container.SchedulerProvider,
                container.RxActionFactory,
                container.PermissionsChecker,
                container.BackgroundService,
                container.PlatformInfo,
                container.WidgetsService));

            ReportsViewModel = new Lazy<ViewModel>(() => new ReportsViewModel(
                container.DataSource,
                container.NavigationService,
                container.InteractorFactory,
                container.SchedulerProvider,
                container.RxActionFactory,
                container.AnalyticsService,
                container.TimeService,
                container.DateRangeShortcutsService));

            CalendarViewModel = new Lazy<ViewModel>(() => new CalendarViewModel(
                container.DataSource,
                container.TimeService,
                container.RxActionFactory,
                container.UserPreferences,
                container.AnalyticsService,
                container.BackgroundService,
                container.InteractorFactory,
                container.SchedulerProvider,
                container.OnboardingStorage,
                container.PermissionsChecker,
                container.SyncManager,
                container.NavigationService));

            Tabs = getViewModels().ToImmutableList();

            SsoLinkResult = ssoLinkResultSubject
                .AsDriver(MainTabBarParameters.SsoLinkResult.NONE, container.SchedulerProvider);
        }

        public override Task Initialize(MainTabBarParameters payload)
        {
            ssoLinkResultSubject.OnNext(payload.LinkResult);
            return base.Initialize(payload);
        }

        private IEnumerable<Lazy<ViewModel>> getViewModels()
        {
            yield return MainViewModel;
            yield return CalendarViewModel;
            yield return ReportsViewModel;
        }
    }
}
