using Realms;
using System;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;
using static Toggl.Shared.PropertySyncStatus;

namespace Toggl.Storage.Realm
{
    internal partial class RealmUser
        : RealmObject, IDatabaseUser, IUpdatable, ISyncable<IUser>
    {
        public string ApiToken { get; set; }

        public DateTimeOffset At { get; set; }

        public long? DefaultWorkspaceId { get; set; }

        //Realm doesn't support enums
        [Ignored]
        public BeginningOfWeek BeginningOfWeek
        {
            get => (BeginningOfWeek)BeginningOfWeekInt;
            set => BeginningOfWeekInt = (int)value;
        }

        public int BeginningOfWeekInt { get; set; }

        [Ignored]
        public Email Email
        {
            get => EmailString.ToEmail();
            set => EmailString = value.ToString();
        }

        [MapTo("Email")]
        public string EmailString { get; set; }

        public string Fullname { get; set; }

        public string ImageUrl { get; set; }

        public string Language { get; set; }

        public string Timezone { get; set; }

        public void PrepareForSyncing()
        {
            SyncStatus = SyncStatus.Syncing;
            changePropertiesSyncStatus(from: SyncNeeded, to: Syncing);
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
            changePropertiesSyncStatus(from: Syncing, to: SyncNeeded);
        }

        public void UpdateSucceeded()
        {
            if (SyncStatus != SyncStatus.SyncNeeded)
                SyncStatus = SyncStatus.InSync;

            changePropertiesSyncStatus(from: Syncing, to: InSync);
        }

        public void SaveSyncResult(IUser entity, Realms.Realm realm)
        {
            ApiToken = entity.ApiToken;
            At = entity.At;
            DefaultWorkspaceId = entity.DefaultWorkspaceId;
            BeginningOfWeek = entity.BeginningOfWeek;
            Email = entity.Email;
            Fullname = entity.Fullname;
            ImageUrl = entity.ImageUrl;
            Language = entity.Language;
            Timezone = entity.Timezone;
            SyncStatus = SyncStatus.InSync;
            DefaultWorkspaceIdSyncStatus = InSync;
            BeginningOfWeekSyncStatus = InSync;
            LastSyncErrorMessage = null;
        }

        private void changePropertiesSyncStatus(PropertySyncStatus from, PropertySyncStatus to)
        {
            if (DefaultWorkspaceIdSyncStatus == from)
                DefaultWorkspaceIdSyncStatus = to;

            if (BeginningOfWeekSyncStatus == from)
                BeginningOfWeekSyncStatus = to;
        }
    }
}
