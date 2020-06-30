using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using System;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;
using EssentialsPlatform = Xamarin.Essentials.Platform;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/ModalActivityTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class SsoLinkActivity : ReactiveActivity<SsoLinkViewModel>
    {
        public SsoLinkActivity() : base(
            Resource.Layout.SsoLinkActivity,
            Resource.Style.ModalActivityTheme,
            Transitions.SlideInFromBottom)
        {
        }

        public SsoLinkActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            linkButton.Rx().Tap()
                .Subscribe(ViewModel.Link.Inputs)
                .DisposedBy(DisposeBag);

            closeButton.Rx().Tap()
                .Subscribe(_ => ViewModel.CloseWithDefaultResult())
                .DisposedBy(DisposeBag);
        }
    }
}
