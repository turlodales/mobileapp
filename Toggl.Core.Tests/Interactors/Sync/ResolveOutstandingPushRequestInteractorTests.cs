using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Sync.Push;
using Xunit;

namespace Toggl.Core.Tests.Interactors.Workspace
{
    public sealed class ResolveOutstandingPushRequestInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useApi,
                bool usePushRequestIdentifier,
                bool useQueryFactory)
            {
                var syncApi = useApi ? Api.SyncApi : null;
                var pushRequestIdentifier = usePushRequestIdentifier ? Database.PushRequestIdentifier : null;
                var queryFactory = useQueryFactory ? QueryFactory : null;

                Action tryingToConstructWithNull = () =>
                    new ResolveOutstandingPushRequestInteractor(
                        syncApi,
                        pushRequestIdentifier,
                        queryFactory);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly ResolveOutstandingPushRequestInteractor interactor;

            public TheExecuteMethod()
            {
                interactor = new ResolveOutstandingPushRequestInteractor(
                    Api.SyncApi, Database.PushRequestIdentifier, QueryFactory);
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotMakeAnyApiCallsIfThereIsNoOutstandingPushRequest()
            {
                Database.PushRequestIdentifier.TryGet(out Arg.Any<Guid>()).Returns(false);

                await interactor.Execute();

                await Api.SyncApi.DidNotReceive().OutstandingPush(Arg.Any<Guid>());
            }

            [Fact, LogIfTooSlow]
            public async Task FinishesInFirstLoopIfTheOutstandingRequestResultIsPulledSuccessfully()
            {
                var id = Guid.NewGuid();
                Database.PushRequestIdentifier.TryGet(out Arg.Any<Guid>()).Returns(
                    x => { x[0] = id; return true; },
                    x => false);

                await interactor.Execute();

                await Api.SyncApi.Received(1).OutstandingPush(Arg.Is(id));
                Database.PushRequestIdentifier.Received().Clear();
            }

            [Fact, LogIfTooSlow]
            public async Task ExitsTheLoopAsSoonAsIfTheOutstandingRequestResultIsPulledSuccessfully()
            {
                var id = Guid.NewGuid();
                Database.PushRequestIdentifier.TryGet(out Arg.Any<Guid>()).Returns(
                    x => { x[0] = id; return true; },
                    x => { x[0] = id; return true; },
                    x => { x[0] = id; return true; },
                    x => false);
                Api.SyncApi.OutstandingPush(Arg.Is(id))
                    .Returns(requestFails(exceptionWhenTimeouts), requestFails(exceptionWhenTimeouts), requestSucceds);

                await interactor.Execute();

                await Api.SyncApi.Received(3).OutstandingPush(Arg.Is(id));
                Received.InOrder(() =>
                {
                    Api.SyncApi.OutstandingPush(Arg.Is(id));
                    Api.SyncApi.OutstandingPush(Arg.Is(id));
                    Api.SyncApi.OutstandingPush(Arg.Is(id));
                    Database.PushRequestIdentifier.Clear();
                });
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotRepeatTheRequestIfThereIsNoRequestWithTheGivenId()
            {
                var id = Guid.NewGuid();
                Database.PushRequestIdentifier.TryGet(out Arg.Any<Guid>()).Returns(
                    x => { x[0] = id; return true; },
                    x => false);
                var notFound = requestFails(new NotFoundException(
                    Substitute.For<Networking.Network.IRequest>(),
                    Substitute.For<Networking.Network.IResponse>()));
                Api.SyncApi.OutstandingPush(Arg.Is(id)).Returns(notFound);

                await interactor.Execute();

                await Api.SyncApi.Received(1).OutstandingPush(Arg.Is(id));
                Database.PushRequestIdentifier.Received().Clear();
            }

            [Fact, LogIfTooSlow]
            public async Task ThrowsWhenOfflineAndKeepsTheRequestId()
            {
                var id = Guid.NewGuid();
                Database.PushRequestIdentifier.TryGet(out Arg.Any<Guid>())
                    .Returns(x => { x[0] = id; return true; });
                Api.SyncApi.OutstandingPush(Arg.Is(id)).Returns(
                    requestFails(new OfflineException()));

                Func<Task> exec = async () => await interactor.Execute();

                await exec.Should().ThrowAsync<OfflineException>();
                Database.PushRequestIdentifier.DidNotReceive().Clear();
            }

            private static Task<IResponse> requestFails(Exception e) =>
                Task.FromException<IResponse>(e);

            private static Task<IResponse> requestSucceds =>
                Task.FromResult<IResponse>(Substitute.For<IResponse>());

            private static OfflineException exceptionWhenTimeouts =>
                new OfflineException(
                    new HttpRequestException("", new TaskCanceledException()));
        }
    }
}
