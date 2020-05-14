using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.DTOs;
using Toggl.Core.Interactors;
using Toggl.Networking.ApiClients;
using Toggl.Networking.Exceptions;
using Toggl.Shared;

namespace Toggl.Core.Sync.V2
{
    public static class SyncManagerSelector
    {
        public static ISyncManager Select(
            IInteractorFactory interactorFactory,
            IPreferencesApi preferencesApi,
            Func<ISyncManager> oldSyncManagerCreator,
            Func<ISyncManager> newSyncManagerCreator)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(preferencesApi, nameof(preferencesApi));
            Ensure.Argument.IsNotNull(oldSyncManagerCreator, nameof(oldSyncManagerCreator));
            Ensure.Argument.IsNotNull(newSyncManagerCreator, nameof(newSyncManagerCreator));

            var preferences = interactorFactory.ObserveCurrentPreferences().Execute().GetAwaiter().Wait();

            var activeSyncManager = preferences.UseNewSync
                ? newSyncManagerCreator()
                : oldSyncManagerCreator();

            /*
             * The active sync manager is chosen by the preferences object.
             * However, fetching the most up-to-date preferences is part of a
             * sync manager's job. If the active sync manager is faulty and it is
             * unable to synchronize the preferences, it is impossible to select
             * a new one via beta flags.
             * 
             * That's why this separate API call is made that will directly
             * query the v9 to ask for the preferences.
             *
             * If the request fails, the app will fall back to the old sync manager.
             */
            _ = checkForActiveSyncManager(interactorFactory, preferencesApi);

            return activeSyncManager;
        }

        private static async Task checkForActiveSyncManager(IInteractorFactory interactorFactory, IPreferencesApi preferencesApi)
        {
            try
            {
                var preferences = await preferencesApi.Get();
                var preferencesDto = new EditPreferencesDTO { UseNewSync = preferences.UseNewSync };
                await interactorFactory.UpdatePreferences(preferencesDto).Execute();
            }
            catch (OfflineException)
            {
                // In case of the offline exception, the sync should not return to the old one.
                // In that case, do nothing.
            }
            catch
            {
                var preferencesDto = new EditPreferencesDTO { UseNewSync = false };
                await interactorFactory.UpdatePreferences(preferencesDto).Execute();
            }
        }
    }
}
