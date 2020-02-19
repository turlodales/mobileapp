using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Core.UI.ViewModels;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Shared.Models;
using Xunit;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public sealed class TermsAndCountryViewModelTests
    {
        public abstract class TermsAndCountryViewModelTest : BaseViewModelWithOutputTests<TermsAndCountryViewModel, ICountry?>
        {
            protected ILocation Location { get; } = Substitute.For<ILocation>();

            protected override TermsAndCountryViewModel CreateViewModel()
                => new TermsAndCountryViewModel(
                    ApiFactory,
                    TimeService,
                    SchedulerProvider,
                    RxActionFactory,
                    NavigationService);

            protected override void AdditionalSetup()
            {
                Location.CountryCode.Returns("LV");
                Location.CountryName.Returns("Latvia");

                Api.Location.Get().ReturnsTaskOf(Location);

                ApiFactory.CreateApiWith(Arg.Any<Credentials>(), Arg.Any<ITimeService>()).Returns(Api);

                var container = new TestDependencyContainer { MockSyncManager = SyncManager };
                TestDependencyContainer.Initialize(container);
            }
        }

        public sealed class TheConstructor : TermsAndCountryViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useApiFactory,
                bool useTimeService,
                bool useSchedulerProvider,
                bool useRxActionFactory,
                bool useNavigationService)
            {
                var apiFactory = useApiFactory ? ApiFactory : null;
                var timeService = useTimeService ? TimeService : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TermsAndCountryViewModel(
                        apiFactory,
                        timeService,
                        schedulerProvider,
                        rxActionFactory,
                        navigationService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : TermsAndCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task GetsTheCurrentLocation()
            {
                await ViewModel.Initialize();

                await Api.Location.Received().Get();
            }

            [Fact, LogIfTooSlow]
            public async Task SetsTheCountryButtonTitleToCountryNameWhenApiCallSucceeds()
            {
                var observer = TestScheduler.CreateObserver<string>();
                ViewModel.CountryButtonTitle.Subscribe(observer);

                await ViewModel.Initialize();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, Resources.SelectCountry),
                    ReactiveTest.OnNext(2, Location.CountryName)
                );
            }

            [Fact, LogIfTooSlow]
            public async Task SetsTheCountryButtonTitleToSelectCountryWhenApiCallFails()
            {
                var observer = TestScheduler.CreateObserver<string>();
                ViewModel.CountryButtonTitle.Subscribe(observer);

                Api.Location.Get().ReturnsThrowingTaskOf(new Exception());

                await ViewModel.Initialize();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, Resources.SelectCountry)
                );
            }

            [Fact, LogIfTooSlow]
            public async Task SetsFailedToGetCountryToTrueWhenApiCallFails()
            {
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsCountryErrorVisible.Subscribe(observer);

                Api.Location.Get().ReturnsThrowingTaskOf(new Exception());

                await ViewModel.Initialize();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false),
                    ReactiveTest.OnNext(2, true)
                );
            }
        }

        public sealed class ThePickCountryMethod : TermsAndCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToSelectCountryViewModelPassingNullIfLocationApiFailed()
            {
                Api.Location.Get().ReturnsThrowingTaskOf(new Exception());
                await ViewModel.Initialize();

                ViewModel.PickCountry.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Navigate<SelectCountryViewModel, long?, long?>(null, ViewModel.View);
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToSelectCountryViewModelPassingCountryIdIfLocationApiSucceeded()
            {
                await ViewModel.Initialize();
                var selectedCountryId = await new GetAllCountriesInteractor()
                    .Execute()
                    .Select(countries => countries.Single(country => country.CountryCode == Location.CountryCode))
                    .Select(country => country.Id);

                ViewModel.PickCountry.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Navigate<SelectCountryViewModel, long?, long?>(selectedCountryId, ViewModel.View);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheCountryButtonTitle()
            {
                var observer = TestScheduler.CreateObserver<string>();
                ViewModel.CountryButtonTitle.Subscribe(observer);

                var selectedCountry = await new GetAllCountriesInteractor()
                    .Execute()
                    .Select(countries => countries.Single(country => country.Id == 1));

                NavigationService
                    .Navigate<SelectCountryViewModel, long?, long?>(Arg.Any<long?>(), ViewModel.View)
                    .Returns(selectedCountry.Id);

                await ViewModel.Initialize();

                ViewModel.PickCountry.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, Resources.SelectCountry),
                    ReactiveTest.OnNext(2, Location.CountryName),
                    ReactiveTest.OnNext(3, selectedCountry.Name)
                );
            }

            [Fact, LogIfTooSlow]
            public async Task SetsFailedToGetCountryToFalse()
            {
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsCountryErrorVisible.Subscribe(observer);

                Api.Location.Get().ReturnsThrowingTaskOf(new Exception());
                NavigationService
                    .Navigate<SelectCountryViewModel, long?, long?>(Arg.Any<long?>(), ViewModel.View)
                    .Returns(1);
                await ViewModel.Initialize();

                ViewModel.PickCountry.Execute();

                TestScheduler.Start();
                observer.Messages.AssertEqual(
                    ReactiveTest.OnNext(1, false),
                    ReactiveTest.OnNext(2, true),
                    ReactiveTest.OnNext(3, false)
                );
            }
        }
    }
}
