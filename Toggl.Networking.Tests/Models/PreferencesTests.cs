using FluentAssertions;
using NSubstitute.Exceptions;
using System;
using Toggl.Networking.Models;
using Toggl.Networking.Serialization;
using Toggl.Shared;
using Xunit;

namespace Toggl.Networking.Tests.Models
{
    public sealed class PreferencesTests
    {
        private string alphaFeaturesJson(bool useSync = false)
            => $"\"alpha_features\":[{{\"code\":\"mobile_sync_client\",\"enabled\":{useSync.ToString().ToLower()}}}]";

        private string getValidIncomingJson(bool useSync = false)
            => $"{{\"timeofday_format\":\"h:mm A\",\"date_format\":\"YYYY-MM-DD\",\"duration_format\":\"improved\",\"CollapseTimeEntries\":true,{alphaFeaturesJson(useSync)}}}";

        private string getValidJson()
            => "{\"timeofday_format\":\"h:mm A\",\"date_format\":\"YYYY-MM-DD\",\"duration_format\":\"improved\",\"CollapseTimeEntries\":true}";

        private Preferences validPreferences => new Preferences
        {
            TimeOfDayFormat = TimeFormat.FromLocalizedTimeFormat("h:mm A"),
            DateFormat = DateFormat.FromLocalizedDateFormat("YYYY-MM-DD"),
            DurationFormat = DurationFormat.Improved,
            CollapseTimeEntries = true,
            UseNewSync = true
        };

        [Fact, LogIfTooSlow]
        public void HasConstructorWhichCopiesValuesFromInterfaceToTheNewInstance()
        {
            var clonedObject = new Preferences(validPreferences);

            clonedObject.Should().NotBeSameAs(validPreferences);
            clonedObject.Should().BeEquivalentTo(validPreferences, options => options.IncludingProperties());
        }

        [Fact, LogIfTooSlow]
        public void CanBeDeserialized()
        {
            SerializationHelper.CanBeDeserialized(getValidIncomingJson(validPreferences.UseNewSync), validPreferences);
        }

        [Fact, LogIfTooSlow]
        public void CanBeSerialized()
        {
            SerializationHelper.CanBeSerialized(getValidJson(), validPreferences, SerializationReason.Post);
        }

        [Fact, LogIfTooSlow]
        public void UseNewSyncIsNotSerialized()
        {
            var serializer = new JsonSerializer();

            var json = serializer.SerializeRoundtrip(validPreferences);

            json.ContainsProperty("alpha_features").Should().BeFalse();
        }

        [Theory, LogIfTooSlow]
        [InlineData(true)]
        [InlineData(false)]
        public void UseNewSyncIsDeserialized(bool useSync)
        {
            var serializer = new JsonSerializer();

            var preferences = serializer.Deserialize<Preferences>(getValidIncomingJson(useSync));

            preferences.UseNewSync.Should().Be(useSync);
        }
    }
}
