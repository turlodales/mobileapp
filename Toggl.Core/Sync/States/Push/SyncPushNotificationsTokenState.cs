using System;
using System.Reactive;
using Toggl.Core.Interactors;
using Toggl.Core.Interactors.PushNotifications;
using Toggl.Core.Services;
using Toggl.Networking;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Settings;

namespace Toggl.Core.Sync.States.Push
{
    public sealed class SyncPushNotificationsTokenState : ISyncState
    {
        private readonly IKeyValueStorage keyValueStorage;
        private readonly ITogglApi togglApi;
        private readonly IPushNotificationsTokenService pushNotificationsTokenService;

        public StateResult Done { get; } = new StateResult();

        public SyncPushNotificationsTokenState(
            IKeyValueStorage keyValueStorage,
            ITogglApi togglApi,
            IPushNotificationsTokenService pushNotificationsTokenService)
        {
            Ensure.Argument.IsNotNull(keyValueStorage, nameof(keyValueStorage));
            Ensure.Argument.IsNotNull(togglApi, nameof(togglApi));
            Ensure.Argument.IsNotNull(pushNotificationsTokenService, nameof(pushNotificationsTokenService));

            this.keyValueStorage = keyValueStorage;
            this.togglApi = togglApi;
            this.pushNotificationsTokenService = pushNotificationsTokenService;
        }

        public IObservable<ITransition> Start()
            => createInteractor().Execute().SelectValue(Done.Transition());

        private IInteractor<IObservable<Unit>> createInteractor()
            => new SubscribeToPushNotificationsInteractor(keyValueStorage, togglApi, pushNotificationsTokenService);
    }
}