using System;
using System.Collections.Generic;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Networking.Tests.Models.Calendar
{
    public sealed class SyncedCalendarsPageTests
    {
        public sealed class TheSyncedCalendarsPageModel
        {
            private string validJson
                => "{\"calendars\":[{\"id\":\"Cal-1\",\"name\":\"Personal\"},{\"id\":\"Cal-2\",\"name\":\"Work\"}],\"next_page_token\":\"next_page\"}";

            private SyncedCalendarsPage validPage => new SyncedCalendarsPage
            {
                calendars = new List<ISyncedCalendar>
                {
                    new SyncedCalendar
                    {
                        SyncId = "Cal-1",
                        Name = "Personal",
                    },
                    new SyncedCalendar
                    {
                        SyncId = "Cal-2",
                        Name = "Work",
                    },
                },
                nextPageToken = "next_page",
            };

            [Fact, LogIfTooSlow]
            public void CanBeDeserialized()
            {
                SerializationHelper.CanBeDeserialized(validJson, validPage);
            }
        }
    }
}
