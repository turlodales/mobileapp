using System;
using System.Reactive;
using System.Threading.Tasks;
using Toggl.Core.UI.Parameters;
using Toggl.Networking;
using Toggl.Networking.Exceptions;
using Toggl.Shared;

namespace Toggl.Core.UI.ViewModels.Extensions
{
    internal static class ViewModelExtensions
    {
        internal static async Task ssoLinkIfNeededAndNavigate<TInput, TOutput>(this ViewModel<TInput, TOutput> viewModel, ITogglApi togglApi, bool isForAccountLinking, Email ssoLinkEmail)
        {
            var parameters = MainTabBarParameters.Default;
            if (isForAccountLinking)
            {
                parameters = MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.SUCCESS);
                try
                {
                    await togglApi.User.LinkSso(ssoLinkEmail, "some_token_123");
                }
                catch (BadSsoEmailException badSsoEmailException)
                {
                    parameters =
                        MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.BAD_EMAIL_ERROR);
                }
                catch (Exception exception)
                {
                    parameters = MainTabBarParameters.withSsoLinkResult(MainTabBarParameters.SsoLinkResult.GENERIC_ERROR);
                }
            }

            await viewModel.Navigate<MainTabBarViewModel, MainTabBarParameters>(parameters);
        }
    }
}