using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Android.OS;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Helper;
using Toggl.Droid.Presentation;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using EssentialsPlatform = Xamarin.Essentials.Platform;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class SsoLoginActivity : ReactiveActivity<SsoViewModel>
    {
        public SsoLoginActivity() : base(
            Resource.Layout.SsoLoginActivity,
            Resource.Style.AppTheme,
            Transitions.SlideInFromBottom)
        {
        }

        public SsoLoginActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void OnResume()
        {
            base.OnResume();
            EssentialsPlatform.OnResume();
        }

        protected override void InitializeBindings()
        {
            EssentialsPlatform.Init(this, Bundle.Empty);

            ViewModel.Email.FirstAsync()
                .Select(email => email.ToString())
                .SubscribeOn(AndroidDependencyContainer.Instance.SchedulerProvider.MainScheduler)
                .Subscribe(emailEditText.Rx().TextObserver(true))
                .DisposedBy(DisposeBag);

            emailEditText.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.Email.Accept)
                .DisposedBy(DisposeBag);

            continueButton.Rx().Tap()
                .Subscribe(ViewModel.Continue.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.EmailErrorMessage
                .Subscribe(emailInputLayout.Rx().ErrorObserver())
                .DisposedBy(DisposeBag);

            ViewModel.ErrorMessage
                .Subscribe(message =>
                {
                    if (message == "")
                    {
                        errorLabel.Visibility = ViewStates.Gone;
                        errorLabel.Text = "";
                    }
                    else
                    {
                        errorLabel.Text = message;
                        errorLabel.Visibility = ViewStates.Visible;
                    }
                })
                .DisposedBy(DisposeBag);

                continueButton.Rx()
                .BindAction(ViewModel.Continue)
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Subscribe(loadingOverlay.Rx().IsVisible(useGone: false))
                .DisposedBy(DisposeBag);

            var isNotLoading = ViewModel.IsLoading.Invert();
            isNotLoading
                .Subscribe(emailInputLayout.Rx().Enabled())
                .DisposedBy(DisposeBag);

            isNotLoading
                .Subscribe(this.Rx().NavigationEnabled())
                .DisposedBy(DisposeBag);

            loadingOverlay.Rx().Tap()
                .Subscribe(CommonFunctions.DoNothing)
                .DisposedBy(DisposeBag);

            this.CancelAllNotifications();
        }
    }
}
