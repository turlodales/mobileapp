using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Mocks;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Sync.Push;
using Xunit;

namespace Toggl.Core.Tests.Interactors.Workspace
{
    public sealed class PushSyncInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useApi,
                bool usePushRequestIdentifier,
                bool useInteractorFactory,
                bool useQueryFactory)
            {
                var syncApi = useApi ? Api.SyncApi : null;
                var pushRequestIdentifier = usePushRequestIdentifier ? Database.PushRequestIdentifier : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var queryFactory = useQueryFactory ? QueryFactory : null;

                Action tryingToConstructWithNull = () =>
                    new PushSyncInteractor(syncApi, pushRequestIdentifier, interactorFactory, queryFactory);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly IInteractorFactory mockInteractorFactory = Substitute.For<IInteractorFactory>();
            private readonly PushSyncInteractor interactor;
            private readonly Request req;

            public TheExecuteMethod()
            {
                interactor = new PushSyncInteractor(
                    Api.SyncApi, Database.PushRequestIdentifier, mockInteractorFactory, QueryFactory);

                req = new Request("user agent name");
                req.CreateTags(new[] { new MockTag() }); // just to make sure that it's not empty
                var prepareInteractor = Substitute.For<IInteractor<Task<Request>>>();
                var task = Task.FromResult(req);
                prepareInteractor.Execute().Returns(task);
                mockInteractorFactory.PreparePushRequest().Returns(prepareInteractor);
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotMakeAnHttpRequestIfThereIsNothingToPush()
            {
                var emptyReq = new Request("user agent name");
                var task = Task.FromResult(emptyReq);
                var prepareInteractor = Substitute.For<IInteractor<Task<Request>>>();
                prepareInteractor.Execute().Returns(task);
                mockInteractorFactory.PreparePushRequest().Returns(prepareInteractor);

                await interactor.Execute();

                await Api.SyncApi.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<Request>());
            }

            [Fact, LogIfTooSlow]
            public async Task SendsTheDirtyDataToTheSyncServer()
            {
                await interactor.Execute();

                await Api.SyncApi.Received().Push(Arg.Any<Guid>(), Arg.Is(req));
            }

            [Fact, LogIfTooSlow]
            public async Task StoresResultsInTheDatabase()
            {
                var res = Substitute.For<IResponse>();
                var throwingTask = Task.FromResult(res);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                await interactor.Execute();

                QueryFactory.Received().ProcessPushResult(Arg.Is(res));
            }

            [Fact, LogIfTooSlow]
            public async Task StoresTheOutstandingPushRequestIdInDatabaseBeforeMakingTheHttpRequest()
            {
                var res = Substitute.For<IResponse>();
                var throwingTask = Task.FromResult(res);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                await interactor.Execute();

                Received.InOrder(() =>
                {
                    Database.PushRequestIdentifier.Set(Arg.Any<Guid>());
                    Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>());
                });
            }

            [Fact, LogIfTooSlow]
            public async Task RemovesTheOutstandingPushRequestIdFromDatabaseWhenCompleted()
            {
                var res = Substitute.For<IResponse>();
                var throwingTask = Task.FromResult(res);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                await interactor.Execute();

                Database.PushRequestIdentifier.Received().Clear();
            }

            [Fact, LogIfTooSlow]
            public async Task SwallowTheExceptionWhenApiRequestTimeouts()
            {
                var throwingTask = Task.FromException<IResponse>(exceptionWhenRequestTimeouts);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                Func<Task> exec = async () => await interactor.Execute();

                await exec.Should().NotThrowAsync();
            }

            [Fact, LogIfTooSlow]
            public async Task RethrowsBadRequestErrorAndRemovesTheOutstandingPushRequestIdWhenItOccurs()
            {
                var (req, res) = reqAndRes;
                var throwingTask = Task.FromException<IResponse>(new BadRequestException(req, res));
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                Func<Task> exec = async () => await interactor.Execute();
                await exec.Should().ThrowAsync<BadRequestException>();

                Received.InOrder(() =>
                {
                    Database.PushRequestIdentifier.Set(Arg.Any<Guid>());
                    Database.PushRequestIdentifier.Clear();
                });
            }

            [Theory, LogIfTooSlow]
            [MemberData(nameof(AllPossibleApiRequestExceptions))]
            public async Task DoesNotRemoveTheOutstandingRequestIdWhenForAnyExceptionOtherThanBadRequestException(Exception exception)
            {
                // skip bad request exception, this test doesn't apply to it
                if (exception is BadRequestException)
                    return;

                var throwingTask = Task.FromException<IResponse>(exception);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                try
                {
                    await interactor.Execute();
                }
                catch
                { }

                Database.PushRequestIdentifier.Received().Set(Arg.Any<Guid>());
                Database.PushRequestIdentifier.DidNotReceive().Clear();
            }

            [Theory, LogIfTooSlow]
            [MemberData(nameof(AllPossibleApiRequestExceptions))]
            public async Task DoesNotTouchTheDatabaseStateIfTheRequestDoesNotComplete(Exception exception)
            {
                var throwingTask = Task.FromException<IResponse>(exception);
                Api.SyncApi.Push(Arg.Any<Guid>(), Arg.Any<Request>()).Returns(throwingTask);

                try
                {
                    await interactor.Execute();
                }
                catch
                { }

                QueryFactory.DidNotReceive().ProcessPushResult(Arg.Any<IResponse>());
            }

            public static IEnumerable<object[]> AllPossibleApiRequestExceptions()
            {
                var (req, res) = reqAndRes;
                return new object[][]
                {
                    new object[] { new BadRequestException(req, res) },
                    new object[] { new UnauthorizedException(req, res) },
                    new object[] { new ForbiddenException(req, res) },
                    new object[] { new InternalServerErrorException(req, res) },
                    new object[] { new OfflineException() },
                    new object[] { exceptionWhenRequestTimeouts },
                };
            }

            private static (Networking.Network.IRequest, Networking.Network.IResponse) reqAndRes
                => (Substitute.For<Networking.Network.IRequest>(), Substitute.For<Networking.Network.IResponse>());

            private static OfflineException exceptionWhenRequestTimeouts
                => new OfflineException(
                    new HttpRequestException(
                        "", new TaskCanceledException()));
        }
    }
}
