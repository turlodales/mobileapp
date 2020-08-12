using Realms;
using System;
using Toggl.Shared.Models;
using Toggl.Storage.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm
{
    internal partial class RealmWorkspace
        : RealmObject, IDatabaseWorkspace, IPushable, ISyncable<IWorkspace>
    {
        public string Name { get; set; }

        public bool Admin { get; set; }

        public DateTimeOffset? SuspendedAt { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }

        public double? DefaultHourlyRate { get; set; }

        public string DefaultCurrency { get; set; }

        public bool OnlyAdminsMayCreateProjects { get; set; }

        public bool OnlyAdminsSeeBillableRates { get; set; }

        public bool OnlyAdminsSeeTeamDashboard { get; set; }

        public bool ProjectsBillableByDefault { get; set; }

        public int Rounding { get; set; }

        public int RoundingMinutes { get; set; }

        public DateTimeOffset At { get; set; }

        public string LogoUrl { get; set; }

        public bool IsInaccessible { get; set; }

        public void SaveSyncResult(IWorkspace entity, Realms.Realm realm)
        {
            Id = entity.Id;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;
            IsDeleted = entity.ServerDeletedAt.HasValue;
            SyncStatus = SyncStatus.InSync;
            LastSyncErrorMessage = null;
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
            IsInaccessible = false;
        }

        public void PushFailed(string errorMessage)
        {
            LastSyncErrorMessage = errorMessage;
            SyncStatus = SyncStatus.SyncFailed;
        }
    }
}
