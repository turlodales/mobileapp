using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Helper;
using Toggl.Droid.Presentation;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class LoginActivity : ReactiveActivity<LoginViewModel>
    {
        public LoginActivity() : base(
            Resource.Layout.LoginActivity,
            Resource.Style.AppTheme,
            Transitions.SlideInFromBottom)
        {
        }

        public LoginActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            ViewModel.Email.FirstAsync()
                .Select(email => email.ToString())
                .SubscribeOn(AndroidDependencyContainer.Instance.SchedulerProvider.MainScheduler)
                .Subscribe(emailEditText.Rx().TextObserver(true))
                .DisposedBy(DisposeBag);

            ViewModel.Password.FirstAsync()
                .Select(password => password.ToString())
                .SubscribeOn(AndroidDependencyContainer.Instance.SchedulerProvider.MainScheduler)
                .Subscribe(passwordEditText.Rx().TextObserver(true))
                .DisposedBy(DisposeBag);

            //Text
            emailEditText.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept)
                .DisposedBy(DisposeBag);

            ViewModel.Password
                .Select(password => password.ToString())
                .Take(1)
                .Subscribe(passwordEditText.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            passwordEditText.Rx().Text()
                .Select(Password.From)
                .Subscribe(ViewModel.Password.Accept)
                .DisposedBy(DisposeBag);

            ViewModel.EmailErrorMessage
                .Subscribe(emailInputLayout.Rx().ErrorObserver())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordErrorMessage
                .Subscribe(passwordInputLayout.Rx().ErrorObserver())
                .DisposedBy(DisposeBag);

            ViewModel.LoginErrorMessage
                .Subscribe(errorLabel.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            signUpLabel.Rx().Tap()
                .Subscribe(_ =>
                {
                    ViewModel.SignUp.Inputs.OnNext(Unit.Default);
                    ViewModel.CloseWithDefaultResult();
                })
                .DisposedBy(DisposeBag);

            loginButton.Rx()
                .BindAction(ViewModel.Login)
                .DisposedBy(DisposeBag);

            forgotPasswordLabel.Rx()
                .BindAction(ViewModel.ForgotPassword)
                .DisposedBy(DisposeBag);

            passwordEditText.Rx().EditorActionSent()
                .Subscribe(ViewModel.Login.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Subscribe(loadingOverlay.Rx().IsVisible(useGone: false))
                .DisposedBy(DisposeBag);

            var isNotLoading = ViewModel.IsLoading.Invert();
            isNotLoading
                .Subscribe(emailInputLayout.Rx().Enabled())
                .DisposedBy(DisposeBag);

            isNotLoading
                .Subscribe(passwordInputLayout.Rx().Enabled())
                .DisposedBy(DisposeBag);

            isNotLoading
                .Subscribe(this.Rx().NavigationEnabled())
                .DisposedBy(DisposeBag);

            loadingOverlay.Rx().Tap()
                .Subscribe(CommonFunctions.DoNothing)
                .DisposedBy(DisposeBag);

            var animatedLoadingMessage = TextHelpers.AnimatedLoadingMessage();
            ViewModel.IsLoading
                .CombineLatest(animatedLoadingMessage, loginButtonTitle)
                .Subscribe(loginButton.Rx().TextObserver(true))
                .DisposedBy(DisposeBag);

            string loginButtonTitle(bool isLoading, string currentLoadingMessage)
                => isLoading
                    ? currentLoadingMessage
                    : Shared.Resources.LoginTitle;

            this.CancelAllNotifications();
        }
    }
}
