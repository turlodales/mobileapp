using System;
using System.Linq;
using Realms;
using Toggl.Storage.Models.Calendar;

namespace Toggl.Storage.Realm.Models.Calendar
{
    public class RealmSyncedCalendarEvent : RealmObject, IDatabaseSyncedCalendarEvent, IUpdatesFrom<IDatabaseSyncedCalendarEvent>, IModifiableId
    {
        public RealmSyncedCalendarEvent() { }

        public RealmSyncedCalendarEvent(IDatabaseSyncedCalendarEvent entity, Realms.Realm realm)
        {
            Id = entity.Id;
            SetPropertiesFrom(entity, realm);
        }

        public long Id { get; set; }

        public long? OriginalId { get; set; }

        public string SyncId { get; set; }

        public string ICalId { get; set; }

        public string Title { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public DateTimeOffset Updated { get; set; }

        public string BackgroundColor { get; set; }

        public string ForegroundColor { get; set; }

        public long CalendarId => Calendar.Id;

        public IDatabaseSyncedCalendar Calendar => RealmCalendar;

        public RealmSyncedCalendar RealmCalendar { get; set; }

        public void SetPropertiesFrom(IDatabaseSyncedCalendarEvent entity, Realms.Realm realm)
        {
            SyncId = entity.SyncId;
            ICalId = entity.ICalId;
            Title = entity.Title;
            StartTime = entity.StartTime;
            EndTime = entity.EndTime;
            Updated = entity.Updated;
            BackgroundColor = entity.BackgroundColor;
            ForegroundColor = entity.ForegroundColor;
            var skipCalendarFetch = entity?.CalendarId == null;
            RealmCalendar = skipCalendarFetch ? null : realm.All<RealmSyncedCalendar>().Single(x => x.Id == entity.CalendarId);
        }

        public void ChangeId(long id)
        {
            Id = id;
        }

    }
}
