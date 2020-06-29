using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Core.Analytics;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Core.UI.Views;
using Toggl.iOS.ViewControllers;
using UIKit;
using static Toggl.Core.UI.ViewModels.DateRangePicker.DateRangePickerViewModel;

namespace Toggl.iOS.Presentation
{
    public sealed class RootPresenter : IosPresenter
    {
        protected override HashSet<Type> AcceptedViewModels { get; } = new HashSet<Type>
        {
            typeof(MainTabBarViewModel),
            typeof(OnboardingViewModel),
            typeof(TokenResetViewModel),
            typeof(OutdatedAppViewModel),
        };

        private HashSet<Type> viewModelsNotWrappedInNavigationController { get; } = new HashSet<Type>
        {
            typeof(MainTabBarViewModel),
            typeof(OnboardingViewModel),
            typeof(OutdatedAppViewModel),
        };

        public RootPresenter(UIWindow window, AppDelegate appDelegate) : base(window, appDelegate)
        {
        }

        protected override void PresentOnMainThread<TInput, TOutput>(ViewModel<TInput, TOutput> viewModel, IView view)
        {
            var rootViewController = !viewModelsNotWrappedInNavigationController.Contains(viewModel.GetType())
                ? ViewControllerLocator.GetNavigationViewController(viewModel)
                : ViewControllerLocator.GetViewController(viewModel);

            var oldRootViewController = Window.RootViewController;
            Window.RootViewController = rootViewController;

            UIView.Transition(
                Window,
                Animation.Timings.EnterTiming,
                UIViewAnimationOptions.TransitionCrossDissolve,
                () => { },
                () => detachOldRootViewController(oldRootViewController)
            );
        }

        public override bool ChangePresentation(IPresentationChange presentationChange)
        {
            var rootViewController = Window.RootViewController;
            if (rootViewController is MainTabBarController mainTabBarController)
            {
                switch (presentationChange)
                {
                    case ShowReportsPresentationChange showReportsPresentationChange:
                        mainTabBarController.SelectedIndex = 1;
                        var navigationController = mainTabBarController.SelectedViewController as UINavigationController;
                        var reportsViewController = navigationController.ViewControllers.First() as ReportsViewController;
                        var reportsViewModel = reportsViewController.ViewModel;

                        var startDate = showReportsPresentationChange.StartDate;
                        var endDate = showReportsPresentationChange.EndDate;
                        var period = showReportsPresentationChange.Period;
                        var workspaceId = showReportsPresentationChange.WorkspaceId;

                        if (workspaceId.HasValue)
                        {
                            reportsViewModel.SelectWorkspaceById(workspaceId.Value);
                        }

                        if (startDate.HasValue && endDate.HasValue)
                        {
                            var result = new DateRangeSelectionResult(
                                new Toggl.Shared.DateRange(
                                    startDate.Value.DateTime,
                                    endDate.Value.DateTime),
                                DateRangeSelectionSource.Siri);
                            reportsViewModel.SetDateRange.Execute(result);
                        }
                        else if (period.HasValue)
                        {
                            var shortcut = IosDependencyContainer.Instance
                                .DateRangeShortcutsService
                                .GetShortcutFrom(period.Value);

                            var result = new DateRangeSelectionResult(
                                shortcut.DateRange,
                                DateRangeSelectionSource.Siri);
                            reportsViewModel.SetDateRange.Execute(result);
                        }

                        return true;

                    case ShowCalendarPresentationChange _:
                        mainTabBarController.SelectedIndex = 2;
                        return true;
                }
            }

            return false;
        }

        private void detachOldRootViewController(UIViewController viewController)
        {
            var viewControllerToDetach = viewController is UINavigationController navigationController
                ? navigationController.ViewControllers.First()
                : viewController;

            switch (viewControllerToDetach)
            {
                case MainTabBarController mainTabBarController:
                    detachViewModel(mainTabBarController.ViewModel);
                    break;
                case OnboardingViewController onboardingViewController:
                    detachViewModel(onboardingViewController.ViewModel);
                    break;
                case LoginViewController loginViewController:
                    detachViewModel(loginViewController.ViewModel);
                    break;
                case SsoLoginViewController ssoLoginViewController:
                    detachViewModel(ssoLoginViewController.ViewModel);
                    break;
                case SsoLinkViewController ssoLinkViewController:
                    detachViewModel(ssoLinkViewController.ViewModel);
                    break;
                case SignUpViewController signupViewController:
                    detachViewModel(signupViewController.ViewModel);
                    break;
                case TokenResetViewController tokenResetViewController:
                    detachViewModel(tokenResetViewController.ViewModel);
                    break;
                case OutdatedAppViewController outdatedAppViewController:
                    detachViewModel(outdatedAppViewController.ViewModel);
                    break;
            }
        }

        private void detachViewModel<TViewModel>(TViewModel viewModel)
            where TViewModel : IViewModel
        {
            viewModel?.DetachView();
            viewModel?.CloseWithDefaultResult();
            viewModel?.ViewDestroyed();
        }
    }
}
