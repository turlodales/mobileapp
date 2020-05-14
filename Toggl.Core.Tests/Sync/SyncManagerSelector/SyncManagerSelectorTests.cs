using FluentAssertions;
using FsCheck.Experimental;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Toggl.Core.DTOs;
using Toggl.Core.Interactors;
using Toggl.Core.Sync;
using Toggl.Core.Sync.V2;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Networking.ApiClients;
using Toggl.Shared.Models;
using Xunit;

namespace Toggl.Core.Tests.Sync
{
    public class SyncManagerSelectorTests
    {
        public class TheSelectMethod
        {
            private IInteractorFactory interactorFactory = Substitute.For<IInteractorFactory>();
            private IPreferencesApi preferencesApi = Substitute.For<IPreferencesApi>();
            private Func<ISyncManager> oldSyncManagerCreator = Substitute.For<Func<ISyncManager>>();
            private Func<ISyncManager> newSyncManagerCreator = Substitute.For<Func<ISyncManager>>();
            private ISyncManager oldSyncManager = Substitute.For<ISyncManager>();
            private ISyncManager newSyncManager = Substitute.For<ISyncManager>();

            public TheSelectMethod()
            {
                oldSyncManagerCreator().Returns(oldSyncManager);
                newSyncManagerCreator().Returns(newSyncManager);
            }

            private IPreferences setupDbPreferences(bool useNewSync)
            {
                var preferences = new MockPreferences() { UseNewSync = useNewSync };

                interactorFactory.ObserveCurrentPreferences().Execute()
                    .Returns(Observable.Return(preferences));

                interactorFactory.UpdatePreferences(Arg.Any<EditPreferencesDTO>()).Execute()
                    .Returns(Observable.Return(preferences));

                return preferences;
            }

            private ISyncManager selectSyncManager()
                => SyncManagerSelector.Select(
                    interactorFactory,
                    preferencesApi,
                    oldSyncManagerCreator,
                    newSyncManagerCreator);

            [Theory, LogIfTooSlow]
            [MethodTestData]
            public void ThrowsIfAnyArgumentIsNull(
                bool useInteractorFactory,
                bool usePreferencesApi,
                bool useOldSyncManagerCreator,
                bool useNewSyncManagerCreator)
            {
                var interactorFactory = useInteractorFactory ? this.interactorFactory : null;
                var preferencesApi = usePreferencesApi ? this.preferencesApi : null;
                var oldSyncManagerCreator = useOldSyncManagerCreator ? this.oldSyncManagerCreator : null;
                var newSyncManagerCreator = useNewSyncManagerCreator ? this.newSyncManagerCreator : null;

                Action action = () => SyncManagerSelector.Select(interactorFactory, preferencesApi, oldSyncManagerCreator, newSyncManagerCreator);

                action.Should().Throw<ArgumentNullException>();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void CallsTheCorrectCreator(bool useNewSync)
            {
                setupDbPreferences(useNewSync);
                var expectedSyncManager = useNewSync ? newSyncManager : oldSyncManager;
                var usedCreator = useNewSync ? newSyncManagerCreator : oldSyncManagerCreator;
                var unusedCreator = useNewSync ? oldSyncManagerCreator : newSyncManagerCreator;

                var syncManager = selectSyncManager();

                usedCreator.Received().Invoke();
                unusedCreator.DidNotReceive().Invoke();
                syncManager.Should().Be(expectedSyncManager);
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async Task UpdatesThePreferencesWithNewValue(bool useNewSync)
            {
                var preferences = setupDbPreferences(useNewSync);
                preferencesApi.Get().Returns(Task.FromResult(preferences));

                var syncManager = selectSyncManager();

                var expectedUpdateArgument = Arg.Is<EditPreferencesDTO>(dto => dto.UseNewSync.ValueOr(false) == useNewSync);
                interactorFactory.Received().UpdatePreferences(expectedUpdateArgument);
                await interactorFactory.UpdatePreferences(Arg.Any<EditPreferencesDTO>()).Received().Execute();
            }

            [Fact, LogIfTooSlow]
            public void FallsBackToOldSyncIfApiCallFails()
            {
                var preferences = setupDbPreferences(true);
                preferencesApi.Get().Throws(new Exception());

                var syncManager = selectSyncManager();

                var expectedUpdateArgument = Arg.Is<EditPreferencesDTO>(dto => dto.UseNewSync.ValueOr(false) == false);
                interactorFactory.Received().UpdatePreferences(expectedUpdateArgument);
            }
        }
    }
}
