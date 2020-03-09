using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.Interactors.Location;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Xamarin.Essentials;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class TermsAndCountryViewModel : ViewModelWithOutput<ICountry?>
    {
        private const string privacyPolicyUrl = "https://toggl.com/legal/privacy/";
        private const string termsOfServiceUrl = "https://toggl.com/legal/terms/";

        private readonly IApiFactory apiFactory;
        private readonly ITimeService timeService;
        private readonly IAnalyticsService analyticsService;

        private readonly BehaviorSubject<string> countryNameSubject = new BehaviorSubject<string>(Resources.SelectCountry);
        private readonly BehaviorSubject<bool> isCountryErrorVisibleSubject = new BehaviorSubject<bool>(false);

        private IDisposable getCountrySubscription;
        private List<ICountry> allCountries;
        private ICountry country;

        public IObservable<string> CountryButtonTitle { get; }
        public IObservable<bool> IsCountryErrorVisible { get; }

        public int IndexOfPrivacyPolicy { get; }
        public int IndexOfTermsOfService { get; }
        public string FormattedDialogText { get; }
        public ViewAction PickCountry { get; }
        public ViewAction ViewTermsOfService { get; }
        public ViewAction ViewPrivacyPolicy { get; }
        public ViewAction Accept { get; }

        public TermsAndCountryViewModel(
            IApiFactory apiFactory,
            ITimeService timeService,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory,
            INavigationService navigationService,
            IAnalyticsService analyticsService)
            : base(navigationService)
        {
            Ensure.Argument.IsNotNull(apiFactory, nameof(apiFactory));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.apiFactory = apiFactory;
            this.timeService = timeService;
            this.analyticsService = analyticsService;

            PickCountry = rxActionFactory.FromAsync(pickCountry);
            ViewPrivacyPolicy = rxActionFactory.FromAsync(openPrivacyPolicy);
            ViewTermsOfService = rxActionFactory.FromAsync(openTermsOfService);
            Accept = rxActionFactory.FromAction(accept);

            FormattedDialogText = string.Format(
                Resources.TermsOfServiceDialogMessage,
                Resources.TermsOfService,
                Resources.PrivacyPolicy);

            IndexOfPrivacyPolicy = FormattedDialogText.IndexOf(Resources.PrivacyPolicy);
            IndexOfTermsOfService = FormattedDialogText.IndexOf(Resources.TermsOfService);

            IsCountryErrorVisible = isCountryErrorVisibleSubject
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            CountryButtonTitle = countryNameSubject
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            allCountries = await new GetAllCountriesInteractor().Execute();

            var api = apiFactory.CreateApiWith(Credentials.None, timeService);
            getCountrySubscription = new GetCurrentLocationInteractor(api)
                .Execute()
                .Select(location => allCountries.First(country => country.CountryCode == location.CountryCode))
                .Subscribe(
                    setCountryIfNeeded,
                    _ => setCountryErrorIfNeeded(),
                    () =>
                    {
                        getCountrySubscription?.Dispose();
                        getCountrySubscription = null;
                    }
                );
        }

        private void accept()
        {
            analyticsService.OnboardingAgreeButtonTapped.Track();
            Close(country);
        }

        private void setCountryIfNeeded(ICountry country)
        {
            if (this.country != null) return;
            this.country = country;
            countryNameSubject.OnNext(country.Name);
        }

        private void setCountryErrorIfNeeded()
        {
            if (country != null) return;

            analyticsService.OnboardingCountryNotSelected.Track();

            isCountryErrorVisibleSubject.OnNext(true);
        }

        private async Task pickCountry()
        {
            getCountrySubscription?.Dispose();
            getCountrySubscription = null;

            var countryId = country?.Id;

            var selectedCountryId = await Navigate<SelectCountryViewModel, long?, long?>(countryId);

            if (selectedCountryId == null)
            {
                setCountryErrorIfNeeded();
                return;
            }

            var selectedCountry = allCountries
                .Single(country => country.Id == selectedCountryId.Value);

            analyticsService.OnboardingSelectedCountry.Track(selectedCountry.CountryCode);

            isCountryErrorVisibleSubject.OnNext(false);
            country = selectedCountry;
            countryNameSubject.OnNext(selectedCountry.Name);
        }

        private Task openPrivacyPolicy()
        {
            analyticsService.OnboardingPrivacyPolicyOpened.Track();
            return Browser.OpenAsync(privacyPolicyUrl, BrowserLaunchMode.External);
        }

        private Task openTermsOfService()
        {
            analyticsService.OnboardingTermsOfServiceOpened.Track();
            return Browser.OpenAsync(termsOfServiceUrl, BrowserLaunchMode.External);
        }
    }
}
