using System.Collections.Generic;
using Accord.Statistics.Kernels;
using Foundation;
using ImageIO;
using Toggl.iOS.Extensions;
using UIKit;

namespace Toggl.iOS
{
    public sealed class OnboardingLoadingView : UIView
    {
        private UIImageView imageView;

        public OnboardingLoadingView()
        {
            BackgroundColor = ColorAssets.OnboardingPage1BackgroundColor;
            imageView = new UIImageView();
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(imageView);
            imageView.CenterXAnchor.ConstraintEqualTo(CenterXAnchor).Active = true;
            imageView.CenterYAnchor.ConstraintEqualTo(CenterYAnchor).Active = true;
            imageView.WidthAnchor.ConstraintEqualTo(48).Active = true;
            imageView.HeightAnchor.ConstraintEqualTo(48).Active = true;
            reloadAnimation();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            reloadAnimation();
        }

        private void reloadAnimation()
        {
            var imageName = TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Light
                ? "spinner-light"
                : "spinner-dark";

            var url = NSBundle.MainBundle.GetUrlForResource(imageName, "gif");

            var imageSource = CGImageSource.FromUrl(url);
            var frameCount = imageSource.ImageCount;
            var duration = (double)0;
            var frames = new List<UIImage>();

            for (int i = 0; i < frameCount; i++)
            {
                var frameProperties = imageSource.GetProperties(i);
                var gifProperties = frameProperties.Dictionary[CGImageProperties.GIFDictionary];
                var frameDuration = (gifProperties.ValueForKey(CGImageProperties.GIFDelayTime) as NSNumber).DoubleValue;
                var frame = new UIImage(imageSource.CreateImage(i, null));
                duration += frameDuration;
                frames.Add(frame);
                gifProperties.Dispose();
            }

            imageView.Image = UIImage.CreateAnimatedImage(frames.ToArray(), duration);
        }
    }
}
