using CoreGraphics;
using static Toggl.Core.UI.Helper.OnboardingConstants;
using Toggl.iOS.Extensions;
using UIKit;

namespace Toggl.Core.UI.Extensions
{
    public static class OnboardingExtensions
    {
        public static void SetUpTooltipShadow(this UIView view)
        {
            view.Layer.ShadowColor = ShadowColor.ToNativeColor().CGColor;
            view.Layer.ShadowOpacity = ShadowOpacity;
            view.Layer.ShadowRadius = ShadowRadius;
            view.Layer.ShadowOffset = new CGSize(ShadowXOffset, ShadowYOffset);
        }
    }
}
