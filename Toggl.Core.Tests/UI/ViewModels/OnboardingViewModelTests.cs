using System;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Core.Analytics;
using Toggl.Core.Exceptions;
using Toggl.Core.Tests.Generators;
using Toggl.Core.UI.Models;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.Views;
using Toggl.Shared;
using Toggl.Shared.Models;
using Toggl.Storage.Settings;
using Xunit;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public class OnboardingViewModelTest : BaseViewModelTests<OnboardingViewModel>
    {
        protected ILastTimeUsageStorage LastTimeUsageStorage { get; } = Substitute.For<ILastTimeUsageStorage>();

        protected override OnboardingViewModel CreateViewModel()
            => new OnboardingViewModel(
                SchedulerProvider,
                PlatformInfo,
                TimeService,
                AnalyticsService,
                UserAccessManager,
                LastTimeUsageStorage,
                RxActionFactory,
                InteractorFactory,
                NavigationService);

        protected override void AdditionalSetup()
        {
            var container = new TestDependencyContainer { MockSyncManager = SyncManager };
            TestDependencyContainer.Initialize(container);
        }

        public sealed class TheConstructor : OnboardingViewModelTest
        {
            [Xunit.Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useSchedulerProvider,
                bool usePlatformInfo,
                bool useTimeService,
                bool useAnalyticsService,
                bool useUserAccessManager,
                bool useLastTimeUsageStorage,
                bool useRxActionFactory,
                bool useInteractorFactory,
                bool useNavigationService)
            {
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;
                var platformInfo = usePlatformInfo ? PlatformInfo : null;
                var timeService = useTimeService  ? TimeService : null;
                var analyticsService = useAnalyticsService  ? AnalyticsService : null;
                var userAccessManager = useUserAccessManager  ? UserAccessManager : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var lastTimeUsageStorage = useLastTimeUsageStorage ? LastTimeUsageStorage : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new OnboardingViewModel(
                        schedulerProvider,
                        platformInfo,
                        timeService,
                        analyticsService,
                        userAccessManager,
                        lastTimeUsageStorage,
                        rxActionFactory,
                        interactorFactory,
                        navigationService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class ContinueWithApple : OnboardingViewModelTest
        {
            // TODO: Add Login with Apple tests here
        }

        public sealed class ContinueWithGoogle : OnboardingViewModelTest
        {
            public ContinueWithGoogle()
            {
                View.GetGoogleToken().Returns(Observable.Return(""));

                var country = Substitute.For<ICountry>();
                country.Id.Returns(1);
                country.Name.Returns("");

                NavigationService
                    .Navigate<TermsAndCountryViewModel, ICountry?>(Arg.Any<IView>())
                    .Returns(country);
            }

            [Fact, LogIfTooSlow]
            public void SetsIsLoadingToTrueWhenContinuing()
            {
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsLoading.Subscribe(observer);

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false),
                    ReactiveTest.OnNext(2, true)
                );
            }

            [Fact, LogIfTooSlow]
            public void LogsInWithGoogleAndNavigatesToMainVM()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Return(Unit.Default));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                NavigationService
                    .Received()
                    .Navigate<MainTabBarViewModel>(ViewModel.View)
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateWhenLoginFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                NavigationService
                    .DidNotReceive()
                    .Navigate<MainViewModel>(ViewModel.View)
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DisplaysErrorWhenLoginFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new UnauthorizedAccessException()));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();

                View.Received()
                    .Alert(
                        Arg.Is(Resources.Oops),
                        Arg.Is(Resources.GenericLoginError),
                        Arg.Is(Resources.Ok))
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DoesntSetIsLoadingToFalseWhenLoginFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new UnauthorizedAccessException()));

                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsLoading.Subscribe(observer);

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false)
                );
            }

            [Fact, LogIfTooSlow]
            public void SignsUpWithGoogleAndNavigatesToMainVM()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                UserAccessManager
                    .SignUpWithGoogle(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>())
                    .Returns(Observable.Return(Unit.Default));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                NavigationService
                    .Received()
                    .Navigate<MainTabBarViewModel>(ViewModel.View)
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotNavigateWhenSignupFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                UserAccessManager
                    .SignUpWithGoogle(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                NavigationService
                    .DidNotReceive()
                    .Navigate<MainViewModel>(ViewModel.View)
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DisplaysErrorWhenSignupFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                UserAccessManager
                    .SignUpWithGoogle(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();

                View.Received()
                    .Alert(
                        Arg.Is(Resources.Oops),
                        Arg.Is(Resources.GenericSignUpError),
                        Arg.Is(Resources.Ok))
                    .Wait();
            }

            [Fact, LogIfTooSlow]
            public void DoesntSetIsLoadingToFalseWhenSignupFails()
            {
                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                UserAccessManager
                    .SignUpWithGoogle(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));

                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsLoading.Subscribe(observer);

                ViewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false)
                );
            }

            [FsCheck.Xunit.Property]
            public void UpdatesTheTimeOfLastLoginWhenLoggingIn(DateTimeOffset now)
            {
                TimeService.CurrentDateTime.Returns(now);

                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Return(Unit.Default));

                var viewModel = CreateViewModel();
                viewModel.AttachView(View);

                viewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                LastTimeUsageStorage
                    .Received()
                    .SetLogin(Arg.Is(now));
            }

            [FsCheck.Xunit.Property]
            public void UpdatesTheTimeOfLastLoginWhenSigningIn(DateTimeOffset now)
            {
                TimeService.CurrentDateTime.Returns(now);

                UserAccessManager
                    .LoginWithGoogle(Arg.Any<string>())
                    .Returns(Observable.Throw<Unit>(new GoogleLoginException(false)));
                UserAccessManager
                    .SignUpWithGoogle(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<string>())
                    .Returns(Observable.Return(Unit.Default));

                var viewModel = CreateViewModel();
                viewModel.AttachView(View);

                viewModel.ContinueWithGoogle.Execute();

                TestScheduler.Start();
                LastTimeUsageStorage
                    .Received()
                    .SetLogin(Arg.Is(now));
            }

            [Fact, LogIfTooSlow]
            public void TracksTheViewedPages()
            {
                TestScheduler.Start();

                ViewModel.ContinueWithEmail.Execute();
                AnalyticsService.Received().OnboardingPagesViewed.Track(Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>());
            }
        }

        public sealed class ContinueWithEmail : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void GoesToSignupWhenNoPreviousLoginHasBeenDetected()
            {
                ViewModel.ContinueWithEmail.Execute();

                TestScheduler.Start();
                NavigationService
                    .Received()
                    .Navigate<SignUpViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(parameter
                            => parameter == CredentialsParameter.Empty
                        ), ViewModel.View
                    ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void GoesToLogInWhenAPreviousLoginHasBeenDetected()
            {
                LastTimeUsageStorage.LastLogin.Returns(DateTimeOffset.Now);

                ViewModel.ContinueWithEmail.Execute();

                TestScheduler.Start();
                NavigationService
                    .Received()
                    .Navigate<LoginViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(parameter
                            => parameter == CredentialsParameter.Empty
                        ), ViewModel.View
                    ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void DoesntSetIsLoadingToTrueWhenContinuing()
            {
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsLoading.Subscribe(observer);

                ViewModel.ContinueWithEmail.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false)
                );
            }

            [Fact, LogIfTooSlow]
            public void TracksTheViewedPages()
            {
                TestScheduler.Start();

                ViewModel.ContinueWithGoogle.Execute();
                AnalyticsService.Received().OnboardingPagesViewed.Track(Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>());
            }
        }

        public sealed class TheOnboardingScrollAction : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void TracksTheOnboardingScrolledEvent()
            {
                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.Right,
                    PageNumber = 0,
                });

                TestScheduler.Start();

                AnalyticsService.Received().OnboardingPageScroll.Track(OnboardingScrollAction.Automatic, OnboardingScrollDirection.Right, 2);
            }

            [Fact, LogIfTooSlow]
            public void TracksFirstPageViewed()
            {
                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.Right,
                    PageNumber = 0,
                });

                TestScheduler.Start();

                ViewModel.ContinueWithGoogle.Execute();
                AnalyticsService.Received().OnboardingPagesViewed.Track(true, false, false);
            }

            [Fact, LogIfTooSlow]
            public void TracksFirstTwoPagesViewed()
            {
                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.None,
                    PageNumber = 0,
                });

                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.Right,
                    PageNumber = 1,
                });

                TestScheduler.Start();

                ViewModel.ContinueWithGoogle.Execute();
                AnalyticsService.Received().OnboardingPagesViewed.Track(true, true, false);
            }

            [Fact, LogIfTooSlow]
            public void TracksAllTheViewedPages()
            {
                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.None,
                    PageNumber = 0,
                });

                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.Right,
                    PageNumber = 1,
                });

                ViewModel.OnOnboardingScroll.Execute(new OnboardingScrollParameters
                {
                    Action = OnboardingScrollAction.Automatic,
                    Direction = OnboardingScrollDirection.Right,
                    PageNumber = 2,
                });

                TestScheduler.Start();

                ViewModel.ContinueWithGoogle.Execute();
                AnalyticsService.Received().OnboardingPagesViewed.Track(true, true, true);
            }
        }
    }
}
