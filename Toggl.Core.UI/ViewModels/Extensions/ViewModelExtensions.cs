using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.Analytics;
using Toggl.Core.Interactors;
using Toggl.Core.UI.Parameters;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;
using Toggl.Storage.Settings;

namespace Toggl.Core.UI.ViewModels.Extensions
{
    internal static class ViewModelExtensions
    {
        internal static async Task ssoLinkIfNeededAndNavigate<TInput, TOutput>(this ViewModel<TInput, TOutput> viewModel, ITogglApi togglApi, IAnalyticsService analyticsService, bool isForAccountLinking, Email ssoLinkEmail, string confirmationCode)
        {
            var parameters = MainTabBarParameters.Default;
            if (isForAccountLinking)
            {
                parameters = MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.SUCCESS);
                try
                {
                    await togglApi.User.LinkSso(ssoLinkEmail, confirmationCode);
                    analyticsService.SsoLinkOutcome.Track("Success");
                }
                catch (BadSsoEmailException badSsoEmailException)
                {
                    analyticsService.SsoLinkOutcome.Track("BadEmail");
                    parameters =
                        MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.BAD_EMAIL_ERROR);
                }
                catch (Exception exception)
                {
                    analyticsService.SsoLinkOutcome.Track("Error");
                    parameters = MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.GENERIC_ERROR);
                }
            }

            await viewModel.Navigate<MainTabBarViewModel, MainTabBarParameters>(parameters);
        }

        internal static async Task onLoggedIn<TInput, TOutput>(this ViewModel<TInput, TOutput> viewModel, ILastTimeUsageStorage lastTimeUsageStorage, IOnboardingStorage onboardingStorage, IInteractorFactory interactorFactory, ITimeService timeService, IAnalyticsService analyticsService)
        {
            lastTimeUsageStorage.SetLogin(timeService.CurrentDateTime);

            onboardingStorage.SetIsNewUser(false);

            interactorFactory.GetCurrentUser().Execute()
                .Select(u => u.Id)
                .Subscribe(analyticsService.SetUserId);

            await UIDependencyContainer.Instance.SyncManager.ForceFullSync();
        }
    }
}