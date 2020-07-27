using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Queries;
using Toggl.Storage.Realm.Models.Calendar;
using RealmDb = Realms.Realm;

namespace Toggl.Storage.Realm.Queries
{
    public class PersistExternalCalendarsDataQuery : IQuery<Unit>
    {
        private Func<RealmDb> realmProvider;
        private Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>> calendarData;

        public PersistExternalCalendarsDataQuery(Func<RealmDb> realmProvider, Dictionary<IExternalCalendar, IEnumerable<IExternalCalendarEvent>> calendarData)
        {
            Ensure.Argument.IsNotNull(realmProvider, nameof(realmProvider));
            Ensure.Argument.IsNotNull(calendarData, nameof(calendarData));
            this.realmProvider = realmProvider;
            this.calendarData = calendarData;
        }

        public Unit Execute()
        {
            var realm = realmProvider();

            using (var transaction = realm.BeginWrite())
            {
                realm.RemoveAll<RealmExternalCalendar>();
                realm.RemoveAll<RealmExternalCalendarEvent>();

                long calendarId = 0;
                long eventId = 0;
                foreach (var (calendar, events) in calendarData)
                {

                    var dbCalendar = new RealmExternalCalendar(calendarId++, calendar, realm);
                    realm.Add(dbCalendar);

                    foreach (var calendarEvent in events)
                    {
                        var dbEvent = new RealmExternalCalendarEvent(eventId++, calendarEvent, realm);
                        dbEvent.RealmCalendar = dbCalendar;
                        realm.Add(dbEvent);
                    }
                }

                transaction.Commit();
            }

            return Unit.Default;
        }
    }
}
