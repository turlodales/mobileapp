﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Extensions;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.Shortcuts;
using Toggl.Core.Sync;
using Toggl.Core.Sync.V2;
using Toggl.Networking;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage;
using Toggl.Storage.Queries;
using Toggl.Storage.Settings;

namespace Toggl.Core
{
    public abstract class DependencyContainer
    {
        private readonly UserAgent userAgent;
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        // Require recreation during login/logout
        private Lazy<ITogglApi> api;
        private Lazy<ITogglDataSource> dataSource;
        private Lazy<ISyncManager> syncManager;
        private Lazy<IInteractorFactory> interactorFactory;

        // Normal dependencies
        private readonly Lazy<IApiFactory> apiFactory;
        private readonly Lazy<ITogglDatabase> database;
        private readonly Lazy<ITimeService> timeService;
        private readonly Lazy<IPlatformInfo> platformInfo;
        private readonly Lazy<IQueryFactory> queryFactory;
        private readonly Lazy<IRatingService> ratingService;
        private readonly Lazy<ICalendarService> calendarService;
        private readonly Lazy<IKeyValueStorage> keyValueStorage;
        private readonly Lazy<ILicenseProvider> licenseProvider;
        private readonly Lazy<IUserPreferences> userPreferences;
        private readonly Lazy<IRxActionFactory> rxActionFactory;
        private readonly Lazy<IAnalyticsService> analyticsService;
        private readonly Lazy<IBackgroundService> backgroundService;
        private readonly Lazy<IOnboardingStorage> onboardingStorage;
        private readonly Lazy<ISchedulerProvider> schedulerProvider;
        private readonly Lazy<INotificationService> notificationService;
        private readonly Lazy<IRemoteConfigService> remoteConfigService;
        private readonly Lazy<IAccessibilityService> accessibilityService;
        private readonly Lazy<IErrorHandlingService> errorHandlingService;
        private readonly Lazy<ILastTimeUsageStorage> lastTimeUsageStorage;
        private readonly Lazy<IApplicationShortcutCreator> shortcutCreator;
        private readonly Lazy<IBackgroundSyncService> backgroundSyncService;
        private readonly Lazy<IAutomaticSyncingService> automaticSyncingService;
        private readonly Lazy<IAccessRestrictionStorage> accessRestrictionStorage;
        private readonly Lazy<ISyncErrorHandlingService> syncErrorHandlingService;
        private readonly Lazy<IFetchRemoteConfigService> fetchRemoteConfigService;
        private readonly Lazy<IUpdateRemoteConfigCacheService> remoteConfigUpdateService;
        private readonly Lazy<IPrivateSharedStorageService> privateSharedStorageService;
        private readonly Lazy<IPushNotificationsTokenService> pushNotificationsTokenService;
        private readonly Lazy<IUnsyncedDataPersistenceService> unsyncedDataPersistenceService;
        private readonly Lazy<IPushNotificationsTokenStorage> pushNotificationsTokenStorage;
        private readonly Lazy<HttpClient> httpClient;

        // Non lazy
        public virtual IUserAccessManager UserAccessManager { get; }
        public ApiEnvironment ApiEnvironment { get; }

        public ISyncManager SyncManager => syncManager.Value;
        public IInteractorFactory InteractorFactory => interactorFactory.Value;

        public IUnauthenticatedTogglApi UnauthenticatedTogglApi;
        public IApiFactory ApiFactory => apiFactory.Value;
        public ITogglDatabase Database => database.Value;
        public ITimeService TimeService => timeService.Value;
        public IQueryFactory QueryFactory => queryFactory.Value;
        public IPlatformInfo PlatformInfo => platformInfo.Value;
        public ITogglDataSource DataSource => dataSource.Value;
        public IRatingService RatingService => ratingService.Value;
        public IKeyValueStorage KeyValueStorage => keyValueStorage.Value;
        public ILicenseProvider LicenseProvider => licenseProvider.Value;
        public IUserPreferences UserPreferences => userPreferences.Value;
        public IRxActionFactory RxActionFactory => rxActionFactory.Value;
        public IAnalyticsService AnalyticsService => analyticsService.Value;
        public IBackgroundService BackgroundService => backgroundService.Value;
        public IOnboardingStorage OnboardingStorage => onboardingStorage.Value;
        public ISchedulerProvider SchedulerProvider => schedulerProvider.Value;
        public IRemoteConfigService RemoteConfigService => remoteConfigService.Value;
        public IAccessibilityService AccessibilityService => accessibilityService.Value;
        public IErrorHandlingService ErrorHandlingService => errorHandlingService.Value;
        public ILastTimeUsageStorage LastTimeUsageStorage => lastTimeUsageStorage.Value;
        public IBackgroundSyncService BackgroundSyncService => backgroundSyncService.Value;
        public IAutomaticSyncingService AutomaticSyncingService => automaticSyncingService.Value;
        public IAccessRestrictionStorage AccessRestrictionStorage => accessRestrictionStorage.Value;
        public ISyncErrorHandlingService SyncErrorHandlingService => syncErrorHandlingService.Value;
        public IFetchRemoteConfigService FetchRemoteConfigService => fetchRemoteConfigService.Value;
        public IUpdateRemoteConfigCacheService UpdateRemoteConfigCacheService => remoteConfigUpdateService.Value;
        public IPrivateSharedStorageService PrivateSharedStorageService => privateSharedStorageService.Value;
        public IPushNotificationsTokenService PushNotificationsTokenService => pushNotificationsTokenService.Value;
        public IUnsyncedDataPersistenceService UnsyncedDataPersistenceService => unsyncedDataPersistenceService.Value;
        public IPushNotificationsTokenStorage PushNotificationsTokenStorage => pushNotificationsTokenStorage.Value;
        public HttpClient HttpClient => httpClient.Value;

        protected DependencyContainer(ApiEnvironment apiEnvironment, UserAgent userAgent)
        {
            this.userAgent = userAgent;

            ApiEnvironment = apiEnvironment;

            database = new Lazy<ITogglDatabase>(CreateDatabase);
            apiFactory = new Lazy<IApiFactory>(CreateApiFactory);
            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            timeService = new Lazy<ITimeService>(CreateTimeService);
            dataSource = new Lazy<ITogglDataSource>(CreateDataSource);
            queryFactory = new Lazy<IQueryFactory>(CreateQueryFactory);
            platformInfo = new Lazy<IPlatformInfo>(CreatePlatformInfo);
            ratingService = new Lazy<IRatingService>(CreateRatingService);
            calendarService = new Lazy<ICalendarService>(CreateCalendarService);
            keyValueStorage = new Lazy<IKeyValueStorage>(CreateKeyValueStorage);
            licenseProvider = new Lazy<ILicenseProvider>(CreateLicenseProvider);
            rxActionFactory = new Lazy<IRxActionFactory>(CreateRxActionFactory);
            userPreferences = new Lazy<IUserPreferences>(CreateUserPreferences);
            analyticsService = new Lazy<IAnalyticsService>(CreateAnalyticsService);
            backgroundService = new Lazy<IBackgroundService>(CreateBackgroundService);
            interactorFactory = new Lazy<IInteractorFactory>(CreateInteractorFactory);
            onboardingStorage = new Lazy<IOnboardingStorage>(CreateOnboardingStorage);
            schedulerProvider = new Lazy<ISchedulerProvider>(CreateSchedulerProvider);
            shortcutCreator = new Lazy<IApplicationShortcutCreator>(CreateShortcutCreator);
            notificationService = new Lazy<INotificationService>(CreateNotificationService);
            remoteConfigService = new Lazy<IRemoteConfigService>(CreateRemoteConfigService);
            accessibilityService = new Lazy<IAccessibilityService>(CreateAccessibilityService);
            errorHandlingService = new Lazy<IErrorHandlingService>(CreateErrorHandlingService);
            lastTimeUsageStorage = new Lazy<ILastTimeUsageStorage>(CreateLastTimeUsageStorage);
            backgroundSyncService = new Lazy<IBackgroundSyncService>(CreateBackgroundSyncService);
            automaticSyncingService = new Lazy<IAutomaticSyncingService>(CreateAutomaticSyncingService);
            accessRestrictionStorage = new Lazy<IAccessRestrictionStorage>(CreateAccessRestrictionStorage);
            syncErrorHandlingService = new Lazy<ISyncErrorHandlingService>(CreateSyncErrorHandlingService);
            fetchRemoteConfigService = new Lazy<IFetchRemoteConfigService>(CreateFetchRemoteConfigService);
            remoteConfigUpdateService = new Lazy<IUpdateRemoteConfigCacheService>(CreateUpdateRemoteConfigCacheService);
            privateSharedStorageService = new Lazy<IPrivateSharedStorageService>(CreatePrivateSharedStorageService);
            pushNotificationsTokenService = new Lazy<IPushNotificationsTokenService>(CreatePushNotificationsTokenService);
            unsyncedDataPersistenceService = new Lazy<IUnsyncedDataPersistenceService>(CreateUnsyncedDataPersistenceService);
            pushNotificationsTokenStorage =
                new Lazy<IPushNotificationsTokenStorage>(CreatePushNotificationsTokenStorage);
            httpClient = new Lazy<HttpClient>(CreateHttpClient);

            UnauthenticatedTogglApi = UnauthenticatedTogglApiFactory.With(
                new ApiConfiguration(apiEnvironment, Credentials.None, userAgent), httpClient.Value);

            api = apiFactory.Select(factory => factory.CreateApiWith(Credentials.None, timeService.Value));
            UserAccessManager = new UserAccessManager(
                apiFactory,
                database,
                privateSharedStorageService,
                timeService,
                platformInfo);

            UserAccessManager
                .UserLoggedIn
                .Subscribe(RecreateLazyDependenciesForLogin)
                .DisposedBy(disposeBag);

            UserAccessManager
                .UserLoggedOut
                .Subscribe(_ => RecreateLazyDependenciesForLogout())
                .DisposedBy(disposeBag);
        }

        protected abstract ITogglDatabase CreateDatabase();
        protected abstract IPlatformInfo CreatePlatformInfo();
        protected abstract IQueryFactory CreateQueryFactory();
        protected abstract IRatingService CreateRatingService();
        protected abstract ICalendarService CreateCalendarService();
        protected abstract IKeyValueStorage CreateKeyValueStorage();
        protected abstract ILicenseProvider CreateLicenseProvider();
        protected abstract IUserPreferences CreateUserPreferences();
        protected abstract IAnalyticsService CreateAnalyticsService();
        protected abstract IOnboardingStorage CreateOnboardingStorage();
        protected abstract ISchedulerProvider CreateSchedulerProvider();
        protected abstract INotificationService CreateNotificationService();
        protected abstract IRemoteConfigService CreateRemoteConfigService();
        protected abstract IAccessibilityService CreateAccessibilityService();
        protected abstract IErrorHandlingService CreateErrorHandlingService();
        protected abstract ILastTimeUsageStorage CreateLastTimeUsageStorage();
        protected abstract IApplicationShortcutCreator CreateShortcutCreator();
        protected abstract IBackgroundSyncService CreateBackgroundSyncService();
        protected abstract IFetchRemoteConfigService CreateFetchRemoteConfigService();
        protected abstract IAccessRestrictionStorage CreateAccessRestrictionStorage();
        protected abstract IPrivateSharedStorageService CreatePrivateSharedStorageService();
        protected abstract IPushNotificationsTokenService CreatePushNotificationsTokenService();

        protected abstract HttpClient CreateHttpClient();

        protected virtual ITimeService CreateTimeService()
            => new TimeService(SchedulerProvider.DefaultScheduler);

        protected virtual IBackgroundService CreateBackgroundService()
            => new BackgroundService(TimeService, AnalyticsService, UpdateRemoteConfigCacheService, UnsyncedDataPersistenceService, InteractorFactory);

        protected virtual IAutomaticSyncingService CreateAutomaticSyncingService()
            => new AutomaticSyncingService(BackgroundService, TimeService, LastTimeUsageStorage);

        protected virtual ISyncErrorHandlingService CreateSyncErrorHandlingService()
            => new SyncErrorHandlingService(ErrorHandlingService);

        protected virtual ITogglDataSource CreateDataSource()
            => new TogglDataSource(Database, TimeService, AnalyticsService, SchedulerProvider);

        protected virtual IRxActionFactory CreateRxActionFactory()
            => new RxActionFactory(SchedulerProvider);

        protected virtual IApiFactory CreateApiFactory()
            => new ApiFactory(ApiEnvironment, userAgent, HttpClient);

        protected virtual IUpdateRemoteConfigCacheService CreateUpdateRemoteConfigCacheService()
            => new UpdateRemoteConfigCacheService(TimeService, KeyValueStorage, FetchRemoteConfigService);

        protected virtual IUnsyncedDataPersistenceService CreateUnsyncedDataPersistenceService()
            => new UnsyncedDataPersistenceService(AnalyticsService, async (textToWrite, writer) =>
            {
                await writer.WriteAsync(textToWrite);
                writer.Close();
            }, InteractorFactory.CreateUnsyncedDataDump());

        protected virtual IPushNotificationsTokenStorage CreatePushNotificationsTokenStorage()
            => new PushNotificationsTokenStorage(KeyValueStorage);

        protected virtual ISyncManager CreateSyncManager()
        {
            Func<ISyncManager> oldSyncManagerCreator = () => TogglSyncManager.CreateSyncManager(
                Database,
                api.Value,
                DataSource,
                TimeService,
                AnalyticsService,
                LastTimeUsageStorage,
                SchedulerProvider.DefaultScheduler,
                AutomaticSyncingService,
                this
            );

            Func<ISyncManager> newSyncManagerCreator = () => new SyncManagerV2(
                AnalyticsService,
                InteractorFactory,
                SchedulerProvider,
                DataSource);

            var syncManager = SyncManagerSelector.Select(
                interactorFactory.Value,
                queryFactory.Value,
                api.Value.Preferences,
                oldSyncManagerCreator,
                newSyncManagerCreator);

            SyncErrorHandlingService.HandleErrorsOf(syncManager);

            return syncManager;
        }

        protected virtual IInteractorFactory CreateInteractorFactory() => new InteractorFactory(
            userAgent,
            api.Value,
            UserAccessManager,
            database.Select(database => database.IdProvider),
            database,
            timeService,
            syncManager,
            platformInfo,
            dataSource,
            calendarService,
            userPreferences,
            analyticsService,
            onboardingStorage,
            notificationService,
            lastTimeUsageStorage,
            shortcutCreator,
            privateSharedStorageService,
            keyValueStorage,
            pushNotificationsTokenService,
            pushNotificationsTokenStorage,
            queryFactory);

        protected virtual void RecreateLazyDependenciesForLogin(ITogglApi api)
        {
            this.api = new Lazy<ITogglApi>(() => api);

            dataSource = new Lazy<ITogglDataSource>(CreateDataSource);
            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            interactorFactory = shortcutCreator.Select(creator =>
            {
                var factory = CreateInteractorFactory();
                Task.Run(() => creator.OnLogin(factory));
                return factory;
            });
        }

        protected virtual void RecreateLazyDependenciesForLogout()
        {
            api = apiFactory.Select(factory => factory.CreateApiWith(Credentials.None, timeService.Value));

            dataSource = new Lazy<ITogglDataSource>(CreateDataSource);
            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            interactorFactory = new Lazy<IInteractorFactory>(CreateInteractorFactory);
        }
    }
}
