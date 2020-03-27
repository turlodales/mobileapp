using System;
using System.Collections.Generic;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.ViewModels.Settings;
using Toggl.Core.UI.Views;
using Toggl.iOS.ViewControllers;
using UIKit;

namespace Toggl.iOS.Presentation
{
    public sealed class OnboardingPresenter : IosPresenter
    {
        protected override HashSet<Type> AcceptedViewModels { get; } = new HashSet<Type>
        {
            typeof(SignUpViewModel),
            typeof(LoginViewModel)
        };

        public OnboardingPresenter(UIWindow window, AppDelegate appDelegate) : base(window, appDelegate)
        {
        }

        protected override void PresentOnMainThread<TInput, TOutput>(ViewModel<TInput, TOutput> viewModel, IView sourceView)
        {
            var viewController = ViewControllerLocator.GetNavigationViewController(viewModel);

            viewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            viewController.PresentationController.Delegate = new PresentationControllerDelegate(
                () => viewModel.CloseWithDefaultResult());

            var topmostViewController = FindPresentedViewController();

            if (topmostViewController is ReactiveNavigationController navigationController)
            {
                switch (navigationController.TopViewController)
                {
                    case LoginViewController loginViewController:
                        dismissCurrentSheetAndPresentTheNewOne(loginViewController, viewController);
                        return;
                    case SignUpViewController signUpViewController:
                        dismissCurrentSheetAndPresentTheNewOne(signUpViewController, viewController);
                        return;
                }
            }

            topmostViewController.PresentViewController(viewController, true, null);
        }

        private static void dismissCurrentSheetAndPresentTheNewOne<T>(
            ReactiveViewController<T> currentViewController,
            UIViewController newViewController)
            where  T : IViewModel
        {
            var presentingViewController = currentViewController.PresentingViewController;
            currentViewController.ViewModel.CloseWithDefaultResult();
            currentViewController.DismissViewController(true, () =>
            {
                presentingViewController.PresentViewController(newViewController, true, null);
            });
        }
    }
}
