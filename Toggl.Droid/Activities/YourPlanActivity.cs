using Android.App;
using Android.Content.PM;
using Android.Runtime;
using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;

namespace Toggl.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class YourPlanActivity : ReactiveActivity<YourPlanViewModel>
    {
        public YourPlanActivity() : base(
            Resource.Layout.YourPlanActivity,
            Resource.Style.AppTheme,
            Transitions.SlideInFromRight)
        {
        }

        public YourPlanActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void InitializeBindings()
        {
            ViewModel.PlanName
                .Subscribe(planName.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            ViewModel.Expiration
                .Subscribe(planExpirationDate.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            ViewModel.Features
                .Subscribe(adapter.Rx().Items())
                .DisposedBy(DisposeBag);

            moreInfo.Rx().Tap()
                .Subscribe(ViewModel.OpenTogglWebpage.Inputs)
                .DisposedBy(DisposeBag);
        }
    }
}
