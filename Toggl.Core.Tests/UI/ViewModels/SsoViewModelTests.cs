using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NSubstitute.ReceivedExtensions;
using Toggl.Core.Analytics;
using Toggl.Core.Models;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.Helpers;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Shared.Models;
using Xamarin.Essentials;
using Xunit;
using Email = Toggl.Shared.Email;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public sealed class SsoViewModelTests
    {
        public abstract class SsoViewModelTest : BaseViewModelTests<SsoViewModel>
        {
            protected Email ValidEmail { get; } = Email.From("person@company.com");
            protected Email InvalidEmail { get; } = Email.From("peon@compan");
            protected Password ValidPassword { get; } = Password.From("123456Xx");
            protected Password InvalidPassword { get; } = Password.From("123");

            protected IUnauthenticatedTogglApi unauthenticatedTogglApi = Substitute.For<IUnauthenticatedTogglApi>();
            protected ISamlConfig samlConfig = Substitute.For<ISamlConfig>();

            protected Func<Uri, Uri, Task<WebAuthenticatorResult>> authenticateFunc =
                Substitute.For<Func<Uri, Uri, Task<WebAuthenticatorResult>>>();

            protected override SsoViewModel CreateViewModel()
                => new SsoViewModel(
                    AnalyticsService,
                    NavigationService,
                    SchedulerProvider,
                    RxActionFactory,
                    unauthenticatedTogglApi,
                    UserAccessManager,
                    LastTimeUsageStorage,
                    OnboardingStorage,
                    InteractorFactory,
                    TimeService,
                    authenticateFunc
                );

            protected override void AdditionalSetup()
            {
                var container = new TestDependencyContainer { MockSyncManager = SyncManager };
                TestDependencyContainer.Initialize(container);
            }
        }

        public sealed class TheConstructor : SsoViewModelTest
        {
            [NUnit.Framework.Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useTimeService,
                bool userRxActionFactory,
                bool useAnalyticsService,
                bool useUserAccessManager,
                bool useSchedulerProvider,
                bool useInteractorFactory,
                bool useNavigationService,
                bool useOnboardingStorage,
                bool useLastTimeUsageStorage,
                bool useUnauthApi,
                bool useAuthFunc)
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new SsoViewModel(
                        useAnalyticsService ? AnalyticsService : null,
                        useNavigationService ? NavigationService : null,
                        useSchedulerProvider ? SchedulerProvider : null,
                        userRxActionFactory ? RxActionFactory : null,
                        useUnauthApi ? unauthenticatedTogglApi : null,
                        useUserAccessManager ? UserAccessManager : null,
                        useLastTimeUsageStorage ? LastTimeUsageStorage : null,
                        useOnboardingStorage ? OnboardingStorage : null,
                        useInteractorFactory ? InteractorFactory : null,
                        useTimeService ? TimeService : null,
                        useAuthFunc ? authenticateFunc : null);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class ContinueAction : SsoViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task GetsSamlConfigForTheGivenEmailAndTriggersAuthFlow()
            {
                var email = Email.From("person@company.com");
                var uri = new Uri("https://toggl.com/sso");
                unauthenticatedTogglApi.Auth.GetSamlConfig(email).Returns(samlConfig);
                samlConfig.SsoUrl.Returns(uri);
                ViewModel.Email.Accept(email);

                ViewModel.Continue.Execute();
                TestScheduler.Start();

                await unauthenticatedTogglApi.Received().Auth.GetSamlConfig(email);
                await authenticateFunc.Received().Invoke(uri, new Uri("togglauth://"));
            }
        }

        public sealed class AuthFlow : SsoViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task FinishesByLoggingInWithApiToken()
            {
                var email = Email.From("person@company.com");
                var uri = new Uri("https://toggl.com/sso");
                var authResult = new WebAuthenticatorResult(new Dictionary<string, string>
                {
                    {"apiToken", "some_api_token"}
                });
                unauthenticatedTogglApi.Auth.GetSamlConfig(email).Returns(samlConfig);
                samlConfig.SsoUrl.Returns(uri);
                ViewModel.Email.Accept(email);
                authenticateFunc.Invoke(Arg.Any<Uri>(), Arg.Any<Uri>()).Returns(authResult);

                ViewModel.Continue.Execute();
                TestScheduler.Start();

                await UserAccessManager.Received(1).LoginWithApiToken("some_api_token");
                await NavigationService.Received(1)
                    .Navigate<MainTabBarViewModel, MainTabBarParameters, Unit>(MainTabBarParameters.Default,
                        ViewModel.View);
            }

            [Fact, LogIfTooSlow]
            public async Task FinishesByLinking()
            {
                var email = Email.From("person@company.com");
                var uri = new Uri("https://toggl.com/sso");
                var authResult = new WebAuthenticatorResult(new Dictionary<string, string>
                {
                    {"confirmation_code", "conf_code"},
                    {"email", "person@company.com"}
                });
                unauthenticatedTogglApi.Auth.GetSamlConfig(email).Returns(samlConfig);
                samlConfig.SsoUrl.Returns(uri);
                ViewModel.Email.Accept(email);
                authenticateFunc.Invoke(Arg.Any<Uri>(), Arg.Any<Uri>()).Returns(authResult);

                ViewModel.Continue.Execute();
                TestScheduler.Start();

                await NavigationService.Received(1)
                    .Navigate<SsoLinkViewModel, SsoLinkParameters, Unit>(
                        SsoLinkParameters.WithEmailAndConfirmationCode(email, "conf_code"), View);
            }

            [Fact, LogIfTooSlow]
            public async Task ProducesErrorIfNoMatchingParamsPresent()
            {
                var email = Email.From("person@company.com");
                var uri = new Uri("https://toggl.com/sso");
                var authResult = new WebAuthenticatorResult(new Dictionary<string, string>
                {
                    {"XXXconfirmation_code", "conf_code"},
                    {"aaaAemail", "person@company.com"}
                });
                unauthenticatedTogglApi.Auth.GetSamlConfig(email).Returns(samlConfig);
                samlConfig.SsoUrl.Returns(uri);
                ViewModel.Email.Accept(email);
                authenticateFunc.Invoke(Arg.Any<Uri>(), Arg.Any<Uri>()).Returns(authResult);
                var errorObserver = TestScheduler.CreateObserver<string>();
                ViewModel.ErrorMessage.Subscribe(errorObserver);
                var expectedErrors = new[]
                {
                    "",
                    Shared.Resources.SingleSignOnError
                };

                ViewModel.Continue.Execute();
                TestScheduler.Start();

                await NavigationService.DidNotReceive()
                    .Navigate<SsoLinkViewModel, SsoLinkParameters, Unit>(
                        SsoLinkParameters.WithEmailAndConfirmationCode(email, "conf_code"), View);
                await NavigationService.DidNotReceive()
                    .Navigate<MainTabBarViewModel, MainTabBarParameters, Unit>(MainTabBarParameters.Default, View);
                errorObserver.Values()
                    .Should()
                    .BeEquivalentTo(expectedErrors);
            }
        }
    }
}