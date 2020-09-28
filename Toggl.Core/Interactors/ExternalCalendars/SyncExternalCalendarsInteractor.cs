using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.Extensions;
using Toggl.Core.Models;
using Toggl.Shared;
using Toggl.Shared.Models.Calendar;
using Toggl.Storage.Settings;

namespace Toggl.Core.Interactors
{
    public sealed class SyncExternalCalendarsInteractor : IInteractor<Task<SyncOutcome>>
    {
        private readonly IInteractorFactory interactorFactory;
        private readonly ITimeService timeService;
        private readonly ILastTimeUsageStorage lastTimeUsageStorage;

        public SyncExternalCalendarsInteractor(IInteractorFactory interactorFactory, ITimeService timeService, ILastTimeUsageStorage lastTimeUsageStorage)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(lastTimeUsageStorage, nameof(lastTimeUsageStorage));
            this.interactorFactory = interactorFactory;
            this.timeService = timeService;
            this.lastTimeUsageStorage = lastTimeUsageStorage;
        }

        public async Task<SyncOutcome> Execute()
        {
            // We are disabling the external calendars sync for the rebranding release
            // since it's still untested
            return SyncOutcome.NoData;
        }
    }
}
