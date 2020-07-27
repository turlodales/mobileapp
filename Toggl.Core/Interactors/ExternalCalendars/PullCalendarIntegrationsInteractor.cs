using System.Collections.Generic;
using System.Threading.Tasks;
using Toggl.Networking;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;

namespace Toggl.Core.Interactors
{
    public sealed class PullCalendarIntegrationsInteractor : IInteractor<Task<List<ICalendarIntegration>>>
    {
        private readonly ITogglApi api;

        public PullCalendarIntegrationsInteractor(ITogglApi api)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            this.api = api;
        }

        public Task<List<ICalendarIntegration>> Execute()
            => api.ExternalCalendars.GetIntegrations();
    }
}
