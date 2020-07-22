using System;
using System.Collections.Generic;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Networking.Tests.Models.Calendar
{
    public sealed class ExternalCalendarEventsPageTests
    {
        public sealed class TheExternalCalendarEventsPageModel
        {
            private string validJson
                => "{\"events\":[{\"id\":\"Event-1\",\"ical_uid\":\"Event-1-iCal\", \"title\":\"Meeting\",\"start_time\":\"2020-07-17T09:37:00+00:00\",\"end_time\":\"2020-07-17T09:37:00+00:00\",\"updated\":\"2020-07-17T09:37:00+00:00\",\"background_color\":\"#ffffff\",\"foreground_color\":\"#ffffff\"},{\"id\":\"Event-2\",\"ical_uid\":\"Event-2-iCal\", \"title\":\"Release\",\"start_time\":\"2020-07-17T09:37:00+00:00\",\"end_time\":\"2020-07-17T09:37:00+00:00\",\"updated\":\"2020-07-17T09:37:00+00:00\",\"background_color\":\"#ffffff\",\"foreground_color\":\"#ffffff\"}],\"next_page_token\":\"next_page\"}";

            private ExternalCalendarEventsPage validPage => new ExternalCalendarEventsPage
            {
                events = new List<IExternalCalendarEvent>
                {
                    new ExternalCalendarEvent
                    {
                        SyncId = "Event-1",
                        ICalId = "Event-1-iCal",
                        Title = "Meeting",
                        StartTime = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
                    },
                    new ExternalCalendarEvent
                    {
                        SyncId = "Event-2",
                        ICalId = "Event-2-iCal",
                        Title = "Release",
                        StartTime = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        EndTime = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        Updated = new DateTimeOffset(2020, 7, 17, 9, 37, 0, TimeSpan.Zero),
                        BackgroundColor = "#ffffff",
                        ForegroundColor = "#ffffff",
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
