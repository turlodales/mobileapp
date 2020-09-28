using System;
using System.Collections.Generic;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Networking.Tests.Models.Calendar
{
    public sealed class ExternalCalendarsPageTests
    {
        public sealed class TheExternalCalendarsPageModel
        {
            private string validJson
                => "{\"calendars\":[{\"id\":\"Cal-1\",\"name\":\"Personal\"},{\"id\":\"Cal-2\",\"name\":\"Work\"}],\"next_page_token\":\"next_page\"}";

            private ExternalCalendarsPage validPage => new ExternalCalendarsPage
            {
                Calendars = new List<IExternalCalendar>
                {
                    new ExternalCalendar
                    {
                        SyncId = "Cal-1",
                        Name = "Personal",
                    },
                    new ExternalCalendar
                    {
                        SyncId = "Cal-2",
                        Name = "Work",
                    },
                },
                NextPageToken = "next_page",
            };

            [Fact, LogIfTooSlow]
            public void CanBeDeserialized()
            {
                SerializationHelper.CanBeDeserialized(validJson, validPage);
            }
        }
    }
}
