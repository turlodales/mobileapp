using System;
using System.Collections.Generic;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.Views;
using Toggl.iOS.ViewControllers;
using UIKit;

namespace Toggl.iOS.Presentation
{
    public class OnboardingPresenter : IosPresenter
    {
        protected override HashSet<Type> AcceptedViewModels { get; } = new HashSet<Type>
        {
            typeof(SignUpViewModel),
            typeof(EmailLoginViewModel)
        };

        public OnboardingPresenter(UIWindow window, AppDelegate appDelegate) : base(window, appDelegate)
        {
        }

        protected override void PresentOnMainThread<TInput, TOutput>(ViewModel<TInput, TOutput> viewModel, IView sourceView)
        {
            UIViewController viewController = ViewControllerLocator.GetNavigationViewController(viewModel);

            viewController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            viewController.PresentationController.Delegate = new PresentationControllerDelegate(
                () => viewModel.CloseWithDefaultResult());

            var topmostViewController = FindPresentedViewController();

            //If login or sign up is presented, dismiss it before presenting the new view
            if (topmostViewController is UINavigationController navigationController)
            {
                if (navigationController.TopViewController is LoginViewController loginViewController
                    || navigationController.TopViewController is SignUpViewController signUpViewController)
                {
                    if (navigationController.TopViewController is LoginViewController l)
                        l.ViewModel.CloseWithDefaultResult();
                    if (navigationController.TopViewController is SignUpViewController s)
                        s.ViewModel.CloseWithDefaultResult();

                    topmostViewController = topmostViewController.PresentingViewController;
                    topmostViewController.PresentedViewController.DismissViewController(true, completionHandler: () =>
                    {
                        topmostViewController.PresentViewController(viewController, true, null);
                    });
                    return;
                }
            }

            topmostViewController.PresentViewController(viewController, true, null);
        }
    }
}
