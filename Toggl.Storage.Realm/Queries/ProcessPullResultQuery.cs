using Realms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Networking.Sync.Pull;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Extensions;
using Toggl.Storage.Realm.Models;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Sync
{
    public class ProcessPullResultQuery : IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;
        private Func<DateTimeOffset> currentTimeProvider;
        private IResponse response;

        private IPreferences preferences = null;
        private IUser user = null;
        private ImmutableList<IWorkspace> workspaces;
        private ImmutableList<ITag> tags;
        private ImmutableList<IClient> clients;
        private ImmutableList<IProject> projects;
        private ImmutableList<ITask> tasks;
        private ImmutableList<ITimeEntry> timeEntries;

        public ProcessPullResultQuery(Func<RealmDb> realmProvider, Func<DateTimeOffset> currentTimeProvider, IResponse response)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(response, nameof(response));
            Ensure.Argument.IsNotNull(currentTimeProvider, nameof(currentTimeProvider));

            this.realmProvider = realmProvider;
            this.response = response;
            this.currentTimeProvider = currentTimeProvider;
        }

        private void unpackResponse()
        {
            user = response.User;
            preferences = response.Preferences;

            workspaces = response.Workspaces;
            tags = response.Tags;
            clients = response.Clients;
            projects = response.Projects;
            tasks = response.Tasks;

            timeEntries = response.TimeEntries;
        }

        public Unit Execute()
        {
            unpackResponse();

            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                processSingletonEntity<RealmUser, IUser>(realm, user);
                processSingletonEntity<RealmPreferences, IPreferences>(realm, preferences);

                processEntities<RealmWorkspace, IWorkspace>(realm, workspaces);
                processEntities<RealmTag, ITag>(realm, tags);
                processEntities<RealmClient, IClient>(realm, clients);
                processEntities<RealmProject, IProject>(realm, projects);
                processEntities<RealmTask, ITask>(realm, tasks);

                processTimeEntries(realm);

                transaction.Commit();
            }

            return Unit.Default;
        }

        private void processTimeEntries(RealmDb realm)
        {
            var serverRunningTimeEntry = (ITimeEntry)null;

            foreach (var timeEntry in timeEntries)
            {
                if (timeEntry.IsRunning())
                    serverRunningTimeEntry = timeEntry;

                var dbTimeEntry = realm.Find<RealmTimeEntry>(timeEntry.Id);

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

            preventMultipleRunningTimeEntries(serverRunningTimeEntry, realm);
        }

        private void preventMultipleRunningTimeEntries(ITimeEntry serverRunningTimeEntry, RealmDb realm)
        {
            var time = currentTimeProvider();

            if (serverRunningTimeEntry != null)
            {
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

        private void addTimeEntry(ITimeEntry timeEntry, RealmDb realm)
        {
            var dbTimeEntry = new RealmTimeEntry();
            dbTimeEntry.SaveSyncResult(timeEntry, realm);
            realm.Add(dbTimeEntry);
        }

        private void updateTimeEntry(ITimeEntry timeEntry, RealmTimeEntry dbTimeEntry, RealmDb realm)
        {
            dbTimeEntry.At = timeEntry.At;
            dbTimeEntry.ServerDeletedAt = timeEntry.ServerDeletedAt;
            dbTimeEntry.RealmUser = realm.Find<RealmUser>(timeEntry.UserId);
            dbTimeEntry.RealmWorkspace = realm.Find<RealmWorkspace>(timeEntry.WorkspaceId);

            // Description
            var commonDescription = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.DescriptionBackup
                : dbTimeEntry.Description;

            dbTimeEntry.DescriptionBackup = dbTimeEntry.Description =
                ThreeWayMerge.Merge(commonDescription, dbTimeEntry.Description, timeEntry.Description);

            // ProjectId
            var commonProjectId = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.ProjectIdBackup
                : dbTimeEntry.ProjectId;

            var projectId = ThreeWayMerge.Merge(commonProjectId, dbTimeEntry.ProjectId, timeEntry.ProjectId, identifierComparison);

            dbTimeEntry.RealmProject = projectId.HasValue
                ? realm.Find<RealmProject>(projectId.Value)
                : null;

            // Billable
            var commonBillable = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.BillableBackup
                : dbTimeEntry.Billable;

            dbTimeEntry.BillableBackup = dbTimeEntry.Billable =
                ThreeWayMerge.Merge(commonBillable, dbTimeEntry.Billable, timeEntry.Billable);

            // Start
            var commonStart = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.StartBackup
                : dbTimeEntry.Start;

            dbTimeEntry.Start = ThreeWayMerge.Merge(commonStart, dbTimeEntry.Start, timeEntry.Start);

            // Duration
            var commonDuration = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.DurationBackup
                : dbTimeEntry.Duration;

            dbTimeEntry.Duration = ThreeWayMerge.Merge(commonDuration, dbTimeEntry.Duration, timeEntry.Duration, identifierComparison);

            // Task
            var commonTaskId = dbTimeEntry.ContainsBackup
                ? dbTimeEntry.TaskIdBackup
                : dbTimeEntry.TaskId;

            var taskId = ThreeWayMerge.Merge(commonTaskId, dbTimeEntry.TaskId, timeEntry.TaskId, identifierComparison);

            dbTimeEntry.RealmTask = taskId.HasValue
                ? realm.Find<RealmTask>(taskId.Value)
                : null;

            // Tag Ids
            var commonTagIds = dbTimeEntry.ContainsBackup
                ? Arrays.NotNullOrEmpty(dbTimeEntry.TagIdsBackup)
                : Arrays.NotNullOrEmpty(dbTimeEntry.TagIds);

            var localTagIds = Arrays.NotNullOrEmpty(dbTimeEntry.TagIds);
            var serverTagIds = Arrays.NotNullOrEmpty(timeEntry.TagIds);

            var tagsIds = ThreeWayMerge.Merge(commonTagIds, localTagIds, serverTagIds, collectionEnumerableComparison); ;

            dbTimeEntry.RealmTags.Clear();
            tagsIds
                .Select(tagId => realm.Find<RealmTag>(tagId))
                .AddTo(dbTimeEntry.RealmTags);

            // the conflict is resolved, the backup is no longer needed until next local change
            dbTimeEntry.ContainsBackup = false;
            dbTimeEntry.LastSyncErrorMessage = null;
            dbTimeEntry.SyncStatus = SyncStatus.InSync;
        }

        private void processEntities<TRealmEntity, TEntity>(RealmDb realm, IEnumerable<TEntity> serverEntities)
            where TRealmEntity : RealmObject, TEntity, ISyncable<TEntity>, new()
            where TEntity : IIdentifiable, IDeletable
        {
            foreach (var entity in serverEntities)
            {
                var dbEntity = realm.Find<TRealmEntity>(entity.Id);

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
                        realm.Remove(dbEntity);
                    else
                        dbEntity.SaveSyncResult(entity, realm);
                }
            }
        }

        private void processSingletonEntity<TRealmEntity, TEntity>(RealmDb realm, TEntity serverEntity)
          where TRealmEntity : RealmObject, TEntity, ISyncable<TEntity>, new()
        {
            var dbEntity = realm.All<TRealmEntity>().SingleOrDefault();

            if (dbEntity == null)
            {
                dbEntity = new TRealmEntity();
                dbEntity.SaveSyncResult(serverEntity, realm);
                realm.Add(dbEntity);
            }
            else
            {
                dbEntity.SaveSyncResult(serverEntity, realm);
            }
        }

        private bool identifierComparison(long? a, long? b) => a == b;

        private bool collectionEnumerableComparison<T>(T[] a, T[] b) => a.SetEquals(b);
    }
}
