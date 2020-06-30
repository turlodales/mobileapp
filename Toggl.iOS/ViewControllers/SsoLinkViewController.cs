using System;
using System.Reactive.Linq;
using AuthenticationServices;
using Foundation;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.iOS.Helper;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class SsoLinkViewController : ReactiveViewController<SsoLinkViewModel>
    {
        public SsoLinkViewController(SsoLinkViewModel vm) : base(vm, nameof(SsoLinkViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var closeButton = new UIBarButtonItem(
                UIImage.FromBundle("icClose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            closeButton.TintColor = ColorAssets.IconTint;

            var backButton = new UIBarButtonItem("",
                UIBarButtonItemStyle.Plain,
                (sender, args) => ViewModel.Close());
            backButton.TintColor = ColorAssets.IconTint;

            NavigationItem.LeftBarButtonItem = closeButton;
            NavigationItem.BackBarButtonItem = backButton;

            LinkButton.Rx()
                .BindAction(ViewModel.Link)
                .DisposedBy(DisposeBag);
        }

        private void prepareViews()
        {
            SsoLinkHeaderLabel.SetTextWithOnboardingAppearance(Resources.AccountWithEmailAlreadyExists, useInverseColor: false);
            SsoLinkDescriptionLabel.SetTextWithOnboardingAppearance(Resources.ToEnableLoginWithSsoLoginWithExistingCredentials, false, false);
            LinkButton.SetTitle(Resources.LoginToEnableSso, UIControlState.Normal);
        }
    }
}

