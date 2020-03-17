using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Reactive.Linq;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateVisible,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class ForgotPasswordActivity : ReactiveActivity<ForgotPasswordViewModel>
    {
        public ForgotPasswordActivity() : base(
            Resource.Layout.ForgotPasswordActivity,
            Resource.Style.AppTheme,
            Transitions.SlideInFromRight)
        { }

        public ForgotPasswordActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            ViewModel.ErrorMessage
                .Subscribe(onErrorMessage)
                .DisposedBy(DisposeBag);

            loginEmailEditText.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.OnNext)
                .DisposedBy(DisposeBag);

            ViewModel.Reset.Executing
                .Subscribe(loadingOverlay.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.Reset.Executing
                .Select(resetting => resetting 
                    ? Shared.Resources.Loading 
                    : Shared.Resources.GetPasswordResetLink)
                .Subscribe(resetPasswordButton.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetSuccessful
                .Where(success => success)
                .Subscribe(_ => showResetPasswordSuccessToast())
                .DisposedBy(DisposeBag);

            ViewModel.PasswordResetSuccessful.Invert()
                .Subscribe(resetPasswordButton.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            ViewModel.Reset.Executing.Invert()
                .CombineLatest(ViewModel.EmailValid, CommonFunctions.And)
                .Subscribe(resetPasswordButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            resetPasswordButton.Rx().Tap()
                .Subscribe(ViewModel.Reset.Inputs)
                .DisposedBy(DisposeBag);
            
            loadingOverlay.Rx().Tap()
                .Subscribe(CommonFunctions.DoNothing)
                .DisposedBy(DisposeBag);

            void onErrorMessage(string errorMessage)
            {
                loginEmail.Error = errorMessage;
            }

            void showResetPasswordSuccessToast()
            {
                loginEmailEditText.RemoveFocus();
                Toast.MakeText(this, Shared.Resources.PasswordResetSuccess, ToastLength.Long).Show();
            }
        }
    }
}
