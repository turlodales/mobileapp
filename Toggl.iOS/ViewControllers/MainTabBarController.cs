using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Extensions;
using Toggl.iOS.Presentation;
using Toggl.iOS.Views;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public sealed partial class MainTabBarController : UITabBarController
    {
        public enum Tab
        {
            Main,
            Reports,
            Calendar
        }

        public MainTabBarViewModel ViewModel { get; set; }
        private IDisposable? ssoLinkResultDisposable;

        private static readonly Dictionary<Type, string> imageNameForType = new Dictionary<Type, string>
        {
            { typeof(MainViewModel), "icTime" },
            { typeof(ReportsViewModel), "icReports" },
            { typeof(CalendarViewModel), "icCalendar" }
        };

        private static readonly Dictionary<Type, string> accessibilityLabels = new Dictionary<Type, string>
        {
            { typeof(MainViewModel), Resources.Timer },
            { typeof(ReportsViewModel), Resources.Reports },
            { typeof(CalendarViewModel), Resources.Calendar }
        };

        public MainTabBarController(MainTabBarViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewControllers = ViewModel.Tabs
                .Select(createTabFor)
                .Apply(Task.WhenAll)
                .GetAwaiter()
                .GetResult();

            ssoLinkResultDisposable = ViewModel.SsoLinkResult
                .DistinctUntilChanged()
                .Subscribe(result =>
                {
                    if (result == MainTabBarParameters.SsoLinkResult.SUCCESS)
                    {
                        this.ShowToast(Resources.SsoLinkSuccess);
                    }
                    else if (result == MainTabBarParameters.SsoLinkResult.BAD_EMAIL_ERROR)
                    {
                        this.ShowToast(Resources.SsoLinkFailure);
                    }
                    else if (result == MainTabBarParameters.SsoLinkResult.GENERIC_ERROR)
                    {
                        this.ShowToast(Resources.SomethingWentWrongTryAgain);
                    }
                });

            async Task<UIViewController> createTabFor(Lazy<ViewModel> lazyViewModel)
            {
                var viewModel = lazyViewModel.Value;
                await viewModel.Initialize();
                var viewController = ViewControllerLocator.GetNavigationViewController(viewModel);
                var childViewModelType = viewModel.GetType();
                viewController.TabBarItem = new UITabBarItem
                {
                    Title = "",
                    Image = UIImage.FromBundle(imageNameForType[childViewModelType]),
                    AccessibilityLabel = accessibilityLabels[childViewModelType]
                };
                return viewController;
            }
        }

        public void AddOnboardingBadgeFor(Tab tab)
        {
            var tabBarButton = TabBar.Subviews.Where(view => view is UIControl).ElementAtOrDefault(indexFor(tab));
            if (tabBarButton == null)
                return;

            var centerX = tabBarButton.Center.X;
            var centerY = tabBarButton.Frame.Bottom - 4;
            var tabBarIndicator = new TabBarIndicator(new CGPoint(centerX, centerY));
            tabBarIndicator.BackgroundColor = ColorAssets.Accent;
            tabBarIndicator.Tag = (int)tab;
            TabBar.AddSubview(tabBarIndicator);
        }

        public void RemoveOnboardingBadgeFrom(Tab tab)
        {
            var tabBarIndicator = TabBar
                .Subviews
                .FirstOrDefault(view => view is TabBarIndicator && view.Tag == (int)tab);
            if (tabBarIndicator == null) return;
            tabBarIndicator.RemoveFromSuperview();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            recalculateTabBarInsets();
            setupAppearance();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ssoLinkResultDisposable?.Dispose();
                ssoLinkResultDisposable = null;
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            TabBar.LayoutSubviews();

            var tabBarButtons = TabBar.Subviews.Where(view => view is UIControl).ToArray();
            TabBar
                .Subviews
                .Where(view => view is TabBarIndicator)
                .ForEach(indicator =>
                {
                    var tab = (Tab)(int)indicator.Tag;
                    var tabBarButton = tabBarButtons[indexFor(tab)];
                    indicator.Center = new CGPoint(tabBarButton.Center.X, tabBarButton.Frame.Bottom - 4);
                });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            recalculateTabBarInsets();
            setupAppearance();
        }

        public override void ItemSelected(UITabBar tabbar, UITabBarItem item)
        {
            var targetViewController = ViewControllers.Single(vc => vc.TabBarItem == item);

            if (targetViewController == SelectedViewController
                && tryGetScrollableController() is IScrollableToTop scrollable)
            {
                scrollable.ScrollToTop();
            }
            else if (targetViewController is ReactiveNavigationController navigationController)
            {
                if (navigationController.TopViewController is IReactiveViewController reactiveViewController)
                    reactiveViewController.DismissFromNavigationController();
            }

            UIViewController tryGetScrollableController()
            {
                if (targetViewController is IScrollableToTop)
                    return targetViewController;

                if (targetViewController is UINavigationController nav)
                    return nav.TopViewController;

                return null;
            }
        }

        private void recalculateTabBarInsets()
        {
            ViewControllers.ToList()
                           .ForEach(vc =>
            {
                if (TraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Compact && !UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    // older devices render tab bar item insets weirdly
                    vc.TabBarItem.ImageInsets = new UIEdgeInsets(6, 0, -6, 0);
                }
                else
                {
                    vc.TabBarItem.ImageInsets = new UIEdgeInsets(0, 0, 0, 0);
                }
            });
        }

        private void setupAppearance()
        {
            TabBar.BackgroundImage = ImageExtension.ImageWithColor(ColorAssets.Background);
            TabBar.SelectedImageTintColor = Colors.TabBar.SelectedImageTintColor.ToNativeColor();
            TabBarItem.TitlePositionAdjustment = new UIOffset(0, 200);
        }

        private int indexFor(Tab tab)
            => tab switch
            {
                Tab.Main => 0,
                Tab.Calendar => 1,
                Tab.Reports => 2
            };
    }
}
