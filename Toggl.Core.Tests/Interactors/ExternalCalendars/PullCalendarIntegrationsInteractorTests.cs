using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Networking.Models.Calendar;
using Toggl.Shared.Models.Calendar;
using Xunit;

namespace Toggl.Core.Tests.Interactors.ExternalCalendars
{
    public sealed class PullCalendarIntegrationsInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useApi)
            {
                Action tryingToConstructWithNull = () =>
                    new PullCalendarIntegrationsInteractor(useApi ? Api : null);

                tryingToConstructWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            public sealed class WhenItSucceeds : BaseInteractorTests
            {
                private List<ICalendarIntegration> integrations = new List<ICalendarIntegration>
                {
                    new CalendarIntegration
                    {
                        Id = 42,
                    },
                    new CalendarIntegration
                    {
                        Id = 1337,
                    },
                };

                [Fact, LogIfTooSlow]
                public async Task ItReturnsTheExpectedIntegrations()
                {
                    Api.ExternalCalendars.GetIntegrations().Returns(integrations);
                    var interactor = new PullCalendarIntegrationsInteractor(Api);
                    var actual = await interactor.Execute();
                    actual.Should().BeEquivalentTo(integrations);
                }
            }

            public sealed class WhenItFails : BaseInteractorTests
            {
                [Fact, LogIfTooSlow]
                public async Task ItThrows()
                {
                    Api.ExternalCalendars.GetIntegrations().ReturnsThrowingTaskOf(new Exception("Something bad happened"));
                    var interactor = new PullCalendarIntegrationsInteractor(Api);

                    Action tryingToExecute = () =>
                        interactor.Execute().Wait();

                    tryingToExecute.Should().Throw<Exception>();
                }
            }
        }
    }
}
