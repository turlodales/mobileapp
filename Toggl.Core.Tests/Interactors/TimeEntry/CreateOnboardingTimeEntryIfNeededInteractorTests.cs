using System;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Models;
using Toggl.Core.Tests.Mocks;
using Toggl.Shared;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Tests.Interactors.TimeEntry
{
    public sealed class CreateOnboardingTimeEntryIfNeededInteractorTests : BaseInteractorTests
    {
        private readonly CreateOnboardingTimeEntryIfNeededInteractor interactor;
        private readonly IInteractorFactory mockInteractorFactory = Substitute.For<IInteractorFactory>();

        public CreateOnboardingTimeEntryIfNeededInteractorTests()
        {
            interactor = new CreateOnboardingTimeEntryIfNeededInteractor(TimeService, OnboardingStorage, mockInteractorFactory);
        }

        [Fact, LogIfTooSlow]
        public async Task DoesNotStartATimeEntryIfOnboardingTimeEntryHasBeenCreatedPreviously()
        {
            OnboardingStorage.OnboardingTimeEntryWasCreated().Returns(true);

            await interactor.Execute();

            mockInteractorFactory.DidNotReceive().CreateTimeEntry(
                Arg.Any<ITimeEntryPrototype>(),
                Arg.Any<TimeEntryStartOrigin>());
        }

        [Fact, LogIfTooSlow]
        public async Task DoesNotStartATimeEntryIfOnboardingWasCompleted()
        {
            OnboardingStorage.OnboardingTimeEntryWasCreated().Returns(false);
            OnboardingStorage.CompletedOnboarding().Returns(true);

            await interactor.Execute();

            mockInteractorFactory.DidNotReceive().CreateTimeEntry(
                Arg.Any<ITimeEntryPrototype>(),
                Arg.Any<TimeEntryStartOrigin>());
        }

        [Fact, LogIfTooSlow]
        public async Task DoesNotStartATimeEntryIfThereAreAnyExistingTimeEntries()
        {
            OnboardingStorage.OnboardingTimeEntryWasCreated().Returns(false);
            OnboardingStorage.CompletedOnboarding().Returns(false);
            var timeEntries = new[] { new MockTimeEntry() };
            mockInteractorFactory
                .GetAllTimeEntriesVisibleToTheUser()
                .Execute()
                .Returns(Observable.Return(timeEntries));

            await interactor.Execute();

            mockInteractorFactory.Received().GetAllTimeEntriesVisibleToTheUser();
            mockInteractorFactory.DidNotReceive().CreateTimeEntry(
                Arg.Any<ITimeEntryPrototype>(),
                Arg.Any<TimeEntryStartOrigin>());
        }

        [Fact, LogIfTooSlow]
        public async Task CreatesATimeEntryWithTheCorrectProperties()
        {
            var defaultWorkspace = new MockWorkspace(12356);
            mockInteractorFactory
                .GetDefaultWorkspace()
                .Execute()
                .Returns(Observable.Return(defaultWorkspace));
            OnboardingStorage.OnboardingTimeEntryWasCreated().Returns(false);
            OnboardingStorage.CompletedOnboarding().Returns(false);

            await interactor.Execute();

            mockInteractorFactory.Received().CreateTimeEntry(
                Arg.Is<ITimeEntryPrototype>(timeEntry => timeEntry.Description == Resources.GettingStartedWithTogglApp
                                                         && timeEntry.StartTime == TimeService.CurrentDateTime
                                                         && timeEntry.IsBillable == false
                                                         && timeEntry.Duration == null
                                                         && timeEntry.ProjectId == null
                                                         && timeEntry.TagIds == null
                                                         && timeEntry.TaskId == null
                                                         && timeEntry.WorkspaceId == defaultWorkspace.Id),
                TimeEntryStartOrigin.OnboardingTimeEntry);

        }

        [Fact, LogIfTooSlow]
        public async Task SetsOnboardingTimeEntryWasCreated()
        {
            OnboardingStorage.OnboardingTimeEntryWasCreated().Returns(false);
            OnboardingStorage.CompletedOnboarding().Returns(false);

            await interactor.Execute();

            OnboardingStorage.Received().SetOnboardingTimeEntryWasCreated();
        }
    }
}
