using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Extensions;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Storage.Settings;

namespace Toggl.Core.Interactors
{
    public sealed class CreateOnboardingTimeEntryIfNeededInteractor : IInteractor<Task<IThreadSafeTimeEntry>>
    {
        private readonly ITimeService timeService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IInteractorFactory interactorFactory;

        public CreateOnboardingTimeEntryIfNeededInteractor(
            ITimeService timeService,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.timeService = timeService;
            this.onboardingStorage = onboardingStorage;
            this.interactorFactory = interactorFactory;
        }

        public async Task<IThreadSafeTimeEntry> Execute()
        {
            if (onboardingStorage.OnboardingTimeEntryWasCreated())
                return null;

            if (onboardingStorage.CompletedOnboarding())
                return null;

            var timeEntriesExist = await interactorFactory
                .GetAllTimeEntriesVisibleToTheUser()
                .Execute()
                .Select(timeEntries => timeEntries.Any());
            if (timeEntriesExist)
                return null;

            var defaultWorkspaceId = await interactorFactory
                .GetDefaultWorkspace()
                .Execute()
                .Select(workspace => workspace.Id);

            var timeEntryPrototype = Resources.GettingStartedWithTogglApp
                .AsTimeEntryPrototype(timeService.CurrentDateTime, defaultWorkspaceId);

            var createdTimeEntry = await interactorFactory
                .CreateTimeEntry(timeEntryPrototype, TimeEntryStartOrigin.OnboardingTimeEntry)
                .Execute();
            onboardingStorage.SetOnboardingTimeEntryWasCreated();
            return createdTimeEntry;
        }
    }
}
