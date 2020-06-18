using Toggl.Shared;

namespace Toggl.Core.UI.Parameters
{
    public sealed class OnboardingParameters
    {
        public static readonly OnboardingParameters Default = new OnboardingParameters();

        public static OnboardingParameters forAccountLinking(Email email)
        {
            return new OnboardingParameters
            {
                IsForAccountLinking = true,
                Email = email
            };
        }

        public bool IsForAccountLinking { get; private set; }
        public Email Email { get; private set; } = Email.Empty;
    }
}