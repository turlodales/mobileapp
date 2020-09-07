using System;
using System.Collections.Generic;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.Views;
using Toggl.iOS.ViewControllers;
using System.Linq;
using UIKit;

namespace Toggl.iOS.Presentation
{
    public class TabPresenter : IosPresenter
    {
        public TabPresenter(UIWindow window, AppDelegate appDelegate) : base(window, appDelegate)
        {
        }

        protected override HashSet<Type> AcceptedViewModels { get; } = new HashSet<Type>
        {
            typeof(MainViewModel)
        };

        protected override void PresentOnMainThread<TInput, TOutput>(ViewModel<TInput, TOutput> viewModel, IView sourceView)
        {
            var window = Window;
            var vc = window.RootViewController;

            var tabBarController = (MainTabBarController)window.RootViewController;

            tabBarController.SelectedViewController = tabBarController.ViewControllers[0];
        }
    }
}
