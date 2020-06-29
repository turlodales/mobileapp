using Toggl.Shared;

namespace Toggl.Core.UI.Parameters
{
    public sealed class MainTabBarParameters
    {
        public static readonly MainTabBarParameters Default = new MainTabBarParameters
        {
            LinkResult = SsoLinkResult.NONE
        };

        public static MainTabBarParameters withSsoLinkResult(SsoLinkResult result)
        {
            return new MainTabBarParameters
            {
                LinkResult = result
            };
        }

        public enum SsoLinkResult
        {
            NONE,
            SUCCESS,
            BAD_EMAIL_ERROR,
            GENERIC_ERROR
        }

        public SsoLinkResult LinkResult { get; private set; }
    }
}