using FluentAssertions;
using System;
using NSubstitute;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Shared;
using Xunit;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public class MainTabViewModelTests
    {
        public abstract class MainTabViewModelTest : BaseViewModelWithInputTests<MainTabBarViewModel, MainTabBarParameters>
        {
            protected override MainTabBarViewModel CreateViewModel()
                => new MainTabBarViewModel(new TestDependencyContainer
                    { MockNavigationService =
                        Substitute.For<INavigationService>(),
                        MockSchedulerProvider = Substitute.For<ISchedulerProvider>()
                    });
        }

        public sealed class TheConstructor : MainTabViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ThrowsIfAnyOfTheArgumentsIsNull()
            {
                Action tryingToConstructWithEmptyParameters =
                    () => new MainTabBarViewModel(null);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<Exception>();
            }
        }
    }
}
