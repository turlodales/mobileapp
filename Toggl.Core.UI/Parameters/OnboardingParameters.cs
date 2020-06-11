namespace Toggl.Core.UI.Parameters
{
    public sealed class OnboardingParameters
    {
        public static readonly OnboardingParameters Default = new OnboardingParameters();

        public static readonly OnboardingParameters ForAccountLinking = new OnboardingParameters
            { IsForAccountLinking = true };

        public bool IsForAccountLinking { get; private set; }
    }
}