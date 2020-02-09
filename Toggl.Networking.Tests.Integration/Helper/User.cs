using System;
using System.Net.Http;
using System.Threading.Tasks;
using Toggl.Networking.Network;
using Toggl.Networking.Tests.Integration.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using static Toggl.Networking.Tests.Integration.Helper.TogglApiFactory;

namespace Toggl.Networking.Tests.Integration
{
    internal static class User
    {
        public static async Task<IUser> Create()
        {
            var (email, password) = generateEmailPassword();
            return await createUser(email, password);
        }

        public static async Task<(Email email, Password password)> CreateEmailPassword()
        {
            var (email, password) = generateEmailPassword();
            await createUser(email, password);
            return (email, password);
        }

        public static async Task ResetApiToken(IUser user)
        {
            var message = AuthorizedRequestBuilder.CreateRequest(Credentials.WithApiToken(user.ApiToken),
                "https://toggl.space/api/v8/reset_token", HttpMethod.Post);

            using (var client = CreateHttpClientForIntegrationTests())
            {
                await client.SendAsync(message);
            }
        }

        private static (Email email, Password password) generateEmailPassword()
            => (RandomEmail.GenerateValid(), "123456".ToPassword());

        private static async Task<IUser> createUser(Email email, Password password)
        {
            var api = TogglApiWith(Credentials.None);
            var timeZone = TimeZoneInfo.Local.Id;
            var user = await api.User.SignUp(email, password, true, 237, timeZone);

            return user;
        }
    }
}
