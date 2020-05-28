using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
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
                var api = useApi ? Api : null;
                var pushRequestIdentifier = usePushRequestIdentifier ? Database.PushRequestIdentifier : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var queryFactory = useQueryFactory ? QueryFactory : null;

                Action tryingToConstructWithNull = () =>
                    new PushSyncInteractor(api, pushRequestIdentifier, interactorFactory, queryFactory);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
        }
    }
}
