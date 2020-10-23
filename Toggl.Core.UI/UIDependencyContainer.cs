using System;
using System.Transactions;
using Toggl.Core.Calendar;
using Toggl.Core.Services;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Services;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.Networking;
using Toggl.Networking.Network;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI
{
    public abstract class UIDependencyContainer : DependencyContainer
    {
        private readonly Lazy<IDeeplinkParser> deeplinkParser;
        private readonly Lazy<ViewModelLoader> viewModelLoader;
        private readonly Lazy<INavigationService> navigationService;
        private readonly Lazy<IPermissionsChecker> permissionsService;
        private Lazy<IWidgetsService> widgetsService;
        private Lazy<IDateRangeShortcutsService> dateRangeShortcutsService;

        public IDeeplinkParser DeeplinkParser => deeplinkParser.Value;
        public ViewModelLoader ViewModelLoader => viewModelLoader.Value;
        public INavigationService NavigationService => navigationService.Value;
        public IPermissionsChecker PermissionsChecker => permissionsService.Value;
        public IWidgetsService WidgetsService => widgetsService.Value;
        public IDateRangeShortcutsService DateRangeShortcutsService => dateRangeShortcutsService.Value;

        public static UIDependencyContainer Instance { get; protected set; }

        protected UIDependencyContainer(ApiEnvironment apiEnvironment, UserAgent userAgent)
            : base(apiEnvironment, userAgent)
        {
            deeplinkParser = new Lazy<IDeeplinkParser>(createDeeplinkParser);
            viewModelLoader = new Lazy<ViewModelLoader>(CreateViewModelLoader);
            navigationService = new Lazy<INavigationService>(CreateNavigationService);
            permissionsService = new Lazy<IPermissionsChecker>(CreatePermissionsChecker);
            widgetsService = new Lazy<IWidgetsService>(CreateWidgetsService);
            dateRangeShortcutsService = new Lazy<IDateRangeShortcutsService>(CreateDateRangeShortcutsService);
        }

        private IDeeplinkParser createDeeplinkParser()
            => new DeeplinkParser();

        protected abstract INavigationService CreateNavigationService();
        protected abstract IPermissionsChecker CreatePermissionsChecker();
        protected abstract IWidgetsService CreateWidgetsService();

        protected virtual ViewModelLoader CreateViewModelLoader()
            => new ViewModelLoader(this);

        protected override IErrorHandlingService CreateErrorHandlingService()
            => new ErrorHandlingService(NavigationService, AccessRestrictionStorage);

        protected override IRemoteConfigService CreateRemoteConfigService()
            => new RemoteConfigService(KeyValueStorage);

        protected virtual IDateRangeShortcutsService CreateDateRangeShortcutsService()
            => new DateRangeShortcutsService(DataSource, TimeService);

        protected override void RecreateLazyDependenciesForLogin(ITogglApi togglApi)
        {
            base.RecreateLazyDependenciesForLogin(togglApi);

            widgetsService = reinitializeService(widgetsService, CreateWidgetsService);
            dateRangeShortcutsService = reinitializeService(dateRangeShortcutsService, CreateDateRangeShortcutsService);
        }

        protected override void RecreateLazyDependenciesForLogout()
        {
            base.RecreateLazyDependenciesForLogout();

            widgetsService = reinitializeService(widgetsService, CreateWidgetsService, forced: true);
        }

        private Lazy<TService> reinitializeService<TService>(Lazy<TService> service, Func<TService> serviceCreator, bool forced = false)
            where TService : IDisposable
        {
            if (forced || service.IsValueCreated)
            {
                service.Value?.Dispose();
                return new Lazy<TService>(serviceCreator);
            }

            return service;
        }
    }
}
