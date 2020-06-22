using Realms;
using System.Linq;
using Toggl.Shared.Extensions;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmClient : IUpdatesFrom<IDatabaseClient>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmClient() { }

        public RealmClient(IDatabaseClient entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseClient entity, Realms.Realm realm)
        {
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            Name = entity.Name;
        }
    }

    internal partial class RealmPreferences : IUpdatesFrom<IDatabasePreferences>
    {
        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmPreferences() { }

        public RealmPreferences(IDatabasePreferences entity, Realms.Realm realm)
        {
            SetPropertiesFrom(entity, realm);
        }

        public void SetPropertiesFrom(IDatabasePreferences entity, Realms.Realm realm)
        {
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            TimeOfDayFormat = entity.TimeOfDayFormat;
            DateFormat = entity.DateFormat;
            DurationFormat = entity.DurationFormat;
            CollapseTimeEntries = entity.CollapseTimeEntries;
            UseNewSync = entity.UseNewSync;

            TimeOfDayFormatSyncStatus = entity.TimeOfDayFormatSyncStatus;
            DurationFormatSyncStatus = entity.DurationFormatSyncStatus;
            DateFormatSyncStatus = entity.DateFormatSyncStatus;
            CollapseTimeEntriesSyncStatus = entity.CollapseTimeEntriesSyncStatus;

            CollapseTimeEntriesBackup = entity.CollapseTimeEntriesBackup;
            DateFormatBackup = entity.DateFormatBackup;
            DurationFormatBackup = entity.DurationFormatBackup;
            TimeOfDayFormatBackup = entity.TimeOfDayFormatBackup;
        }
    }

    internal partial class RealmProject : IUpdatesFrom<IDatabaseProject>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmProject() { }

        public RealmProject(IDatabaseProject entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseProject entity, Realms.Realm realm)
        {
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipClientFetch = entity?.ClientId == null || entity.ClientId == 0;
            RealmClient = skipClientFetch ? null : realm.All<RealmClient>().Single(x => x.Id == entity.ClientId || x.OriginalId == entity.ClientId);
            Name = entity.Name;
            IsPrivate = entity.IsPrivate;
            Active = entity.Active;
            Color = entity.Color;
            Billable = entity.Billable;
            Template = entity.Template;
            AutoEstimates = entity.AutoEstimates;
            EstimatedHours = entity.EstimatedHours;
            Rate = entity.Rate;
            Currency = entity.Currency;
            ActualHours = entity.ActualHours;
        }
    }

    internal partial class RealmTag : IUpdatesFrom<IDatabaseTag>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmTag() { }

        public RealmTag(IDatabaseTag entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseTag entity, Realms.Realm realm)
        {
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            Name = entity.Name;
        }
    }

    internal partial class RealmTask : IUpdatesFrom<IDatabaseTask>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmTask() { }

        public RealmTask(IDatabaseTask entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseTask entity, Realms.Realm realm)
        {
            At = entity.At;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            Name = entity.Name;
            var skipProjectFetch = entity?.ProjectId == null || entity.ProjectId == 0;
            RealmProject = skipProjectFetch ? null : realm.All<RealmProject>().Single(x => x.Id == entity.ProjectId || x.OriginalId == entity.ProjectId);
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipUserFetch = entity?.UserId == null || entity.UserId == 0;
            RealmUser = skipUserFetch ? null : realm.All<RealmUser>().Single(x => x.Id == entity.UserId || x.OriginalId == entity.UserId);
            EstimatedSeconds = entity.EstimatedSeconds;
            Active = entity.Active;
            TrackedSeconds = entity.TrackedSeconds;
        }
    }

    internal partial class RealmTimeEntry : IUpdatesFrom<IDatabaseTimeEntry>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmTimeEntry() { }

        public RealmTimeEntry(IDatabaseTimeEntry entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseTimeEntry entity, Realms.Realm realm)
        {
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            var skipProjectFetch = entity?.ProjectId == null || entity.ProjectId == 0;
            RealmProject = skipProjectFetch ? null : realm.All<RealmProject>().SingleOrDefault(x => x.Id == entity.ProjectId || x.OriginalId == entity.ProjectId);
            var skipTaskFetch = RealmProject == null || entity?.TaskId == null || entity.TaskId == 0;
            RealmTask = skipTaskFetch ? null : realm.All<RealmTask>().SingleOrDefault(x => x.Id == entity.TaskId || x.OriginalId == entity.TaskId);
            Billable = entity.Billable;
            Start = entity.Start;
            Duration = entity.Duration;
            Description = entity.Description;

            var tags = entity.TagIds?.Select(id =>
                realm.All<RealmTag>().Single(x => x.Id == id || x.OriginalId == id)) ?? new RealmTag[0];
            RealmTags.Clear();
            tags.ForEach(RealmTags.Add);

            var skipUserFetch = entity?.UserId == null || entity.UserId == 0;
            RealmUser = skipUserFetch ? null : realm.All<RealmUser>().Single(x => x.Id == entity.UserId || x.OriginalId == entity.UserId);

            IsDeletedSyncStatus = entity.IsDeletedSyncStatus;
            BillableSyncStatus = entity.BillableSyncStatus;
            DescriptionSyncStatus = entity.DescriptionSyncStatus;
            DurationSyncStatus = entity.DurationSyncStatus;
            WorkspaceIdSyncStatus = entity.WorkspaceIdSyncStatus;
            ProjectIdSyncStatus = entity.ProjectIdSyncStatus;
            StartSyncStatus = entity.StartSyncStatus;
            TaskIdSyncStatus = entity.TaskIdSyncStatus;
            TagIdsSyncStatus = entity.TagIdsSyncStatus;

            IsDeletedBackup = entity.IsDeletedBackup;
            BillableBackup = entity.BillableBackup;
            DescriptionBackup = entity.DescriptionBackup;
            DurationBackup = entity.DurationBackup;
            WorkspaceIdBackup = entity.WorkspaceIdBackup;
            ProjectIdBackup = entity.ProjectIdBackup;
            StartBackup = entity.StartBackup;
            TaskIdBackup = entity.TaskIdBackup;
            TagIdsBackup.Clear();

            if (entity.TagIdsBackup != null)
            {
                foreach (var tagId in entity.TagIdsBackup)
                    TagIdsBackup.Add(tagId);
            }
        }
    }

    internal partial class RealmUser : IUpdatesFrom<IDatabaseUser>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmUser() { }

        public RealmUser(IDatabaseUser entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseUser entity, Realms.Realm realm)
        {
            At = entity.At;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            ApiToken = entity.ApiToken;
            DefaultWorkspaceId = entity.DefaultWorkspaceId;
            Email = entity.Email;
            Fullname = entity.Fullname;
            BeginningOfWeek = entity.BeginningOfWeek;
            Language = entity.Language;
            ImageUrl = entity.ImageUrl;
            Timezone = entity.Timezone;

            DefaultWorkspaceIdSyncStatus = entity.DefaultWorkspaceIdSyncStatus;
            BeginningOfWeekSyncStatus = entity.BeginningOfWeekSyncStatus;

            DefaultWorkspaceIdBackup = entity.DefaultWorkspaceIdBackup;
            BeginningOfWeekBackup = entity.BeginningOfWeekBackup;
        }
    }

    internal partial class RealmWorkspace : IUpdatesFrom<IDatabaseWorkspace>, IModifiableId
    {
        [Indexed]
        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public int SyncStatusInt { get; set; }

        [Ignored]
        public SyncStatus SyncStatus
        {
            get { return (SyncStatus)SyncStatusInt; }
            set { SyncStatusInt = (int)value; }
        }

        public string LastSyncErrorMessage { get; set; }

        public RealmWorkspace() { }

        public RealmWorkspace(IDatabaseWorkspace entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

        public void SetPropertiesFrom(IDatabaseWorkspace entity, Realms.Realm realm)
        {
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.IsDeleted;
            SyncStatus = entity.SyncStatus;
            LastSyncErrorMessage = entity.LastSyncErrorMessage;
            Name = entity.Name;
            Admin = entity.Admin;
            SuspendedAt = entity.SuspendedAt;
            DefaultHourlyRate = entity.DefaultHourlyRate;
            DefaultCurrency = entity.DefaultCurrency;
            OnlyAdminsMayCreateProjects = entity.OnlyAdminsMayCreateProjects;
            OnlyAdminsSeeBillableRates = entity.OnlyAdminsSeeBillableRates;
            OnlyAdminsSeeTeamDashboard = entity.OnlyAdminsSeeTeamDashboard;
            ProjectsBillableByDefault = entity.ProjectsBillableByDefault;
            Rounding = entity.Rounding;
            RoundingMinutes = entity.RoundingMinutes;
            LogoUrl = entity.LogoUrl;
            IsInaccessible = entity.IsInaccessible;
        }
    }

    internal partial class RealmWorkspaceFeature : IUpdatesFrom<IDatabaseWorkspaceFeature>
    {
        public RealmWorkspaceFeature() { }

        public RealmWorkspaceFeature(IDatabaseWorkspaceFeature entity, Realms.Realm realm)
        {
            SetPropertiesFrom(entity, realm);
        }

        public void SetPropertiesFrom(IDatabaseWorkspaceFeature entity, Realms.Realm realm)
        {
            FeatureId = entity.FeatureId;
            Enabled = entity.Enabled;
        }
    }

    internal partial class RealmWorkspaceFeatureCollection : IUpdatesFrom<IDatabaseWorkspaceFeatureCollection>
    {
        public RealmWorkspaceFeatureCollection() { }

        public RealmWorkspaceFeatureCollection(IDatabaseWorkspaceFeatureCollection entity, Realms.Realm realm)
        {
            SetPropertiesFrom(entity, realm);
        }

        public void SetPropertiesFrom(IDatabaseWorkspaceFeatureCollection entity, Realms.Realm realm)
        {
            var skipWorkspaceFetch = entity?.WorkspaceId == null || entity.WorkspaceId == 0;
            RealmWorkspace = skipWorkspaceFetch ? null : realm.All<RealmWorkspace>().Single(x => x.Id == entity.WorkspaceId || x.OriginalId == entity.WorkspaceId);
            RealmWorkspaceFeatures.Clear();
            foreach (var oneOfFeatures in entity.Features)
            {
                var oneOfRealmFeatures = RealmWorkspaceFeature.FindOrCreate(oneOfFeatures, realm);
                RealmWorkspaceFeatures.Add(oneOfRealmFeatures);
            }
        }
    }
}
