using Realms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Toggl.Networking.Sync.Pull;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using RealmDb = Realms.Realm;
using static Toggl.Storage.Realm.Sync.ThreeWayMerge;

namespace Toggl.Storage.Realm.Sync
{
    internal abstract class WritePulledDataTransaction
    {
        protected void WriteUsingThreeWayMerge(RealmDb realm, Func<DateTimeOffset> now, IResponse response)
        {
            processUser(realm, response.User);
            processPreferences(realm, response.Preferences);

            processEntities<RealmWorkspace, IWorkspace>(realm, response.Workspaces, customDeletionProcess: makeWorkspaceInaccessible);
            processWorkspaceFeatures(realm, response.WorkspaceFeatures);
            processEntities<RealmTag, ITag>(realm, response.Tags);
            processEntities<RealmClient, IClient>(realm, response.Clients);
            processEntities<RealmProject, IProject>(realm, response.Projects, customDeletionProcess: removeRelatedTasks);
            processEntities<RealmTask, ITask>(realm, response.Tasks);

            processTimeEntries(realm, response.TimeEntries);

            var serverRunningTimeEntry = response.TimeEntries.SingleOrDefault(te => !te.ServerDeletedAt.HasValue && te.IsRunning());
            preventMultipleRunningTimeEntries(serverRunningTimeEntry, now, realm);
        }

        private static void processTimeEntries(RealmDb realm, ImmutableList<ITimeEntry> timeEntries)
        {
            foreach (var timeEntry in timeEntries)
            {
                var dbTimeEntry = realm.GetById<RealmTimeEntry>(timeEntry.Id);

                if (dbTimeEntry == null)
                {
                    if (timeEntry.ServerDeletedAt.HasValue)
                        continue;

                    addTimeEntry(timeEntry, realm);
                    continue;
                }

                if (timeEntry.ServerDeletedAt.HasValue)
                    realm.Remove(dbTimeEntry);
                else
                    updateTimeEntry(timeEntry, dbTimeEntry, realm);
            }
        }

        private static void preventMultipleRunningTimeEntries(ITimeEntry serverRunningTimeEntry, Func<DateTimeOffset> now, RealmDb realm)
        {
            var time = now();

            if (serverRunningTimeEntry != null && !isIrrelevantForSyncing(serverRunningTimeEntry, realm))
            {
                // Is it still running? it could have been stopped by 3WM or it could have been deleted from the database.
                // If so, we don't want to stop the other locally running TE;
                var dbServerRunningTimeEntry = realm.GetById<RealmTimeEntry>(serverRunningTimeEntry.Id);
                if (dbServerRunningTimeEntry == null || !dbServerRunningTimeEntry.IsRunning())
                    return;

                var otherLocallyRunningTimeEntries = realm.All<RealmTimeEntry>()
                    .Where(te => te.Duration == null && te.Id != serverRunningTimeEntry.Id)
                    .ToList();

                foreach (var runningTimeEntry in otherLocallyRunningTimeEntries)
                {
                    var duration = (int)(time - runningTimeEntry.Start).TotalSeconds;
                    runningTimeEntry.DurationBackup = runningTimeEntry.Duration = duration;
                    runningTimeEntry.SyncStatus = SyncStatus.SyncNeeded;
                }
            }
        }

        private static void addTimeEntry(ITimeEntry timeEntry, RealmDb realm)
        {
            var dbTimeEntry = new RealmTimeEntry();
            dbTimeEntry.SaveSyncResult(timeEntry, realm);
            realm.Add(dbTimeEntry);
        }

        private static void updateTimeEntry(ITimeEntry timeEntry, RealmTimeEntry dbTimeEntry, RealmDb realm)
        {
            // Temporary hack: the list of entities from the server always contains the currently running TE
            // even if this entity hasn't change since the `since` timestamp. We need to filter it out manually.
            if (isIrrelevantForSyncing(timeEntry, realm)) return;

            dbTimeEntry.SaveSyncResult(timeEntry, realm);
        }

        private static void processUser(RealmDb realm, IUser user)
        {
            var dbUser = realm.All<RealmUser>().SingleOrDefault();

            dbUser.ApiToken = user.ApiToken;
            dbUser.At = user.At;
            dbUser.Email = user.Email;
            dbUser.Fullname = user.Fullname;
            dbUser.ImageUrl = user.ImageUrl;
            dbUser.Language = user.Language;
            dbUser.Timezone = user.Timezone;

            var wasDirty = dbUser.SyncStatus == SyncStatus.SyncNeeded;
            
            (dbUser.DefaultWorkspaceIdSyncStatus, dbUser.DefaultWorkspaceId) =
                Resolve(
                    dbUser.DefaultWorkspaceIdSyncStatus,
                    dbUser.DefaultWorkspaceId,
                    dbUser.DefaultWorkspaceIdBackup,
                    user.DefaultWorkspaceId);

            (dbUser.BeginningOfWeekSyncStatus, dbUser.BeginningOfWeek) =
                ((PropertySyncStatus, BeginningOfWeek))Resolve(
                    dbUser.BeginningOfWeekSyncStatus,
                    (int)dbUser.BeginningOfWeek,
                    (int)dbUser.BeginningOfWeekBackup,
                    (int)user.BeginningOfWeek);

            dbUser.LastSyncErrorMessage = null;

            var hasAtLeastOneDirtyProperty =
                dbUser.DefaultWorkspaceIdSyncStatus == PropertySyncStatus.SyncNeeded
                || dbUser.BeginningOfWeekSyncStatus == PropertySyncStatus.SyncNeeded;
            dbUser.SyncStatus = wasDirty && hasAtLeastOneDirtyProperty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        private static void processPreferences(RealmDb realm, IPreferences preferences)
        {
            var dbPreferences = realm.All<RealmPreferences>().SingleOrDefault();

            if (dbPreferences == null)
            {
                dbPreferences = new RealmPreferences();
                dbPreferences.SaveSyncResult(preferences, realm);
                realm.Add(dbPreferences);
                return;
            }

            var wasDirty = dbPreferences.SyncStatus == SyncStatus.SyncNeeded;

            dbPreferences.UseNewSync = preferences.UseNewSync;

            (dbPreferences.TimeOfDayFormatSyncStatus, dbPreferences.TimeOfDayFormat) =
                Resolve(
                    dbPreferences.TimeOfDayFormatSyncStatus,
                    dbPreferences.TimeOfDayFormat,
                    dbPreferences.TimeOfDayFormatBackup,
                    preferences.TimeOfDayFormat);

            (dbPreferences.DateFormatSyncStatus, dbPreferences.DateFormat) =
                Resolve(
                    dbPreferences.DateFormatSyncStatus,
                    dbPreferences.DateFormat,
                    dbPreferences.DateFormatBackup,
                    preferences.DateFormat);

            (dbPreferences.DurationFormatSyncStatus, dbPreferences.DurationFormat) =
                ((PropertySyncStatus, DurationFormat))Resolve(
                    dbPreferences.DurationFormatSyncStatus,
                    (int)dbPreferences.DurationFormat,
                    (int)dbPreferences.DurationFormatBackup,
                    (int)preferences.DurationFormat);

            (dbPreferences.CollapseTimeEntriesSyncStatus, dbPreferences.CollapseTimeEntries) =
                Resolve(
                    dbPreferences.CollapseTimeEntriesSyncStatus,
                    dbPreferences.CollapseTimeEntries,
                    dbPreferences.CollapseTimeEntriesBackup,
                    preferences.CollapseTimeEntries);

            dbPreferences.LastSyncErrorMessage = null;

            var hasAtLeastOneDirtyProperty =
                dbPreferences.TimeOfDayFormatSyncStatus == PropertySyncStatus.SyncNeeded
                || dbPreferences.DateFormatSyncStatus == PropertySyncStatus.SyncNeeded
                || dbPreferences.DurationFormatSyncStatus == PropertySyncStatus.SyncNeeded
                || dbPreferences.CollapseTimeEntriesSyncStatus == PropertySyncStatus.SyncNeeded;

            dbPreferences.SyncStatus = wasDirty && hasAtLeastOneDirtyProperty
                ? SyncStatus.SyncNeeded
                : SyncStatus.InSync;
        }

        private static void processWorkspaceFeatures(RealmDb realm, ImmutableList<IWorkspaceFeatureCollection> workspaceFeatures)
        {
            foreach (var collection in workspaceFeatures)
            {
                var dbCollection = realm.Find<RealmWorkspaceFeatureCollection>(collection.WorkspaceId);
                dbCollection ??= new RealmWorkspaceFeatureCollection();
                dbCollection.SetPropertiesFrom(collection, realm);
                realm.Add(dbCollection, update: true);
            }
        }

        private static void makeWorkspaceInaccessible(RealmDb realm, RealmWorkspace workspace)
        {
            workspace.IsInaccessible = true;
        }

        private static void removeRelatedTasks(RealmDb realm, RealmProject project)
        {
            project.RealmTasks
                .ToList()
                .ForEach(realm.Remove);

            realm.Remove(project);
        }

        private static void processEntities<TRealmEntity, TEntity>(RealmDb realm, IEnumerable<TEntity> serverEntities, Action<RealmDb, TRealmEntity> customDeletionProcess = null)
            where TRealmEntity : RealmObject, TEntity, ISyncable<TEntity>, new()
            where TEntity : IIdentifiable, IDeletable
        {
            foreach (var entity in serverEntities)
            {
                var dbEntity = realm.GetById<TRealmEntity>(entity.Id);

                var isServerDeleted = entity.ServerDeletedAt.HasValue;

                if (dbEntity == null)
                {
                    if (isServerDeleted)
                        continue;

                    dbEntity = new TRealmEntity();
                    dbEntity.SaveSyncResult(entity, realm);
                    realm.Add(dbEntity);
                }
                else
                {
                    if (isServerDeleted)
                    {
                        if (customDeletionProcess != null)
                        {
                            customDeletionProcess?.Invoke(realm, dbEntity);
                        }
                        else
                        {
                            realm.Remove(dbEntity);
                        }
                    }
                    else
                    {
                        dbEntity.SaveSyncResult(entity, realm);
                    }
                }
            }
        }

        private static bool isIrrelevantForSyncing(ITimeEntry timeEntry, RealmDb realm)
        {
            var timeEntriesSinceId = SinceParameterStorage.IdFor<ITimeEntry>();
            if (!timeEntriesSinceId.HasValue)
                throw new Exception("Time entries since parameter ID is not defined.");

            var since = realm.GetById<RealmSinceParameter>(timeEntriesSinceId.Value);
            return since != null && since.Since != null && timeEntry.At < since.Since.Value;
        }
    }
}
