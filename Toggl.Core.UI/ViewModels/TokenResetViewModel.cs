using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Shared.Extensions.Reactive;
using Toggl.Storage.Settings;
using Xamarin.Essentials;
using Email = Toggl.Shared.Email;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class TokenResetViewModel : ViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IUserAccessManager userAccessManager;
        private readonly IInteractorFactory interactorFactory;
        private IDisposable needsSyncDisposable;
        private IDisposable emailDisposable;
        
        private readonly BehaviorRelay<bool> needsSync = new BehaviorRelay<bool>(false);
        private readonly BehaviorRelay<Email> emailRelay = new BehaviorRelay<Email>(Shared.Email.Empty);

        public IObservable<Email> Email { get; }
        public IObservable<bool> HasError { get; }
        public IObservable<string> Error { get; }
        public IObservable<bool> NextIsEnabled { get; }
        public ISubject<string> Password { get; } = new BehaviorSubject<string>(string.Empty);

        public ViewAction Done { get; private set; }
        public ViewAction SignOut { get; private set; }

        public TokenResetViewModel(
            IUserAccessManager userAccessManager,
            ITogglDataSource dataSource,
            INavigationService navigationService,
            IUserPreferences userPreferences,
            IAnalyticsService analyticsService,
            ISchedulerProvider schedulerProvider,
            IRxActionFactory rxActionFactory,
            IInteractorFactory interactorFactory)
        : base(navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(userAccessManager, nameof(userAccessManager));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));

            this.dataSource = dataSource;
            this.userAccessManager = userAccessManager;
            this.interactorFactory = interactorFactory;

            Done = rxActionFactory.FromObservable(done);
            SignOut = rxActionFactory.FromAsync(signout);

            Email = emailRelay.AsDriver(schedulerProvider);

            Error = Done.Errors
                .Select(transformException);

            HasError = Error
                .Select(error => !string.IsNullOrEmpty(error))
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            NextIsEnabled = Password
                .Select(Shared.Password.From)
                .CombineLatest(Done.Executing, (password, isExecuting) => password.IsValid && !isExecuting)
                .DistinctUntilChanged()
                .AsDriver(schedulerProvider);

            needsSyncDisposable = this.dataSource
                .HasUnsyncedData()
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .Subscribe(isUnsynced => needsSync.Accept(isUnsynced));
            
            emailDisposable = this.dataSource.User.Current
                .Take(1)
                .Select(u => u.Email)
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .Subscribe(emailRelay.Accept);
        }

        private async Task signout()
        {
            if (needsSync.Value)
            {
                var userConfirmedLoggingOut = await askToLogOut();
                if (!userConfirmedLoggingOut)
                    return;
            }

            await interactorFactory.Logout(LogoutSource.TokenReset).Execute();
            await Navigate<LoginViewModel, CredentialsParameter>(CredentialsParameter.Empty);
        }

        private IObservable<Unit> done() =>
            Password
                .FirstAsync()
                .Select(Shared.Password.From)
                .ThrowIf(password => !password.IsValid, new InvalidOperationException())
                .SelectMany(userAccessManager.RefreshToken)
                .Do(onLogin)
                .SelectUnit();

        private void onLogin()
        {
            Navigate<MainTabBarViewModel>();
        }

        private string transformException(Exception ex)
        {
            return ex is ForbiddenException
                ? Resources.IncorrectPassword
                : Resources.GenericLoginError;
        }

        private IObservable<bool> askToLogOut()
            => View.Confirm(
                Resources.AreYouSure,
                Resources.SettingsUnsyncedMessage,
                Resources.SettingsDialogButtonSignOut,
                Resources.Cancel);

        public override void ViewDestroyed()
        {
            base.ViewDestroyed();
            emailDisposable?.Dispose();
            needsSyncDisposable?.Dispose();
            emailDisposable = null;
            needsSyncDisposable = null;
        }
    }
}
