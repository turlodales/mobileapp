using System;
using CoreGraphics;
using Toggl.Core.UI.ViewModels.Calendar;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers.Calendar
{
    public sealed partial class ConnectCalendarsPopupViewController : ReactiveViewController<ConnectCalendarsPopupViewModel>
    {
        public ConnectCalendarsPopupViewController(ConnectCalendarsPopupViewModel viewModel)
            : base(viewModel, nameof(ConnectCalendarsPopupViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TitleLabel.Text = Resources.ConnectCalendarsTitle;
            MessageLabel.Text = Resources.ConnectCalendarsMessage;
            ConnectCalendarsButton.SetTitle(Resources.ConnectCalendarsButtonTitle, UIControlState.Normal);

            PreferredContentSize = new CGSize(0, ConnectCalendarsButton.Frame.Bottom + 40);

            CloseButton.Rx().Tap()
                .Subscribe(_ => ViewModel.Close(false))
                .DisposedBy(DisposeBag);

            ConnectCalendarsButton.Rx().Tap()
                .Subscribe(_ => ViewModel.Close(true))
                .DisposedBy(DisposeBag);
        }
    }
}

