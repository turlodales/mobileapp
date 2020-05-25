using FluentAssertions;
using Toggl.Networking.Serialization;
using Toggl.Networking.Sync.Push;
using Xunit;

namespace Toggl.Networking.Tests.Sync.Push
{
    public sealed class MetaClassesTests
    {
        private JsonSerializer serializer => new JsonSerializer();

        [Theory, LogIfTooSlow]
        [InlineData(1)]
        [InlineData(12)]
        [InlineData(54345435)]
        public void CreateMetaSerializesCorrectly(long id)
        {
            var createMeta = new CreateMeta(id);

            var json = serializer.SerializeRoundtrip(createMeta);
            json.GetLong("client_assigned_id").Should().Be(id);
        }

        [Theory, LogIfTooSlow]
        [InlineData(1, 12)]
        [InlineData(12, 254987)]
        [InlineData(55, 1244)]
        public void DeleteMetaSerializesCorrectly(long id, long workspaceId)
        {
            var createMeta = new ModificationMeta(id, workspaceId);

            var json = serializer.SerializeRoundtrip(createMeta);

            json.GetLong("id").Should().Be(id);
            json.GetLong("workspace_id").Should().Be(workspaceId);
        }
    }
}
