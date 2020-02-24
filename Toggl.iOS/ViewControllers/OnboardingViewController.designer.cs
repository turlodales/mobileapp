// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.iOS.ViewControllers
{
	[Register ("OnboardingViewController")]
	partial class OnboardingViewController
	{
		[Outlet]
		UIKit.UIStackView ButtonsStackView { get; set; }

		[Outlet]
		UIKit.UIButton ContinueWithEmailButton { get; set; }

		[Outlet]
		UIKit.UIButton ContinueWithGoogleButton { get; set; }

		[Outlet]
		UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		UIKit.UIImageView TogglLogo { get; set; }

		[Outlet]
		UIKit.UIView TogglmanView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ButtonsStackView != null) {
				ButtonsStackView.Dispose ();
				ButtonsStackView = null;
			}

			if (ContinueWithEmailButton != null) {
				ContinueWithEmailButton.Dispose ();
				ContinueWithEmailButton = null;
			}

			if (ContinueWithGoogleButton != null) {
				ContinueWithGoogleButton.Dispose ();
				ContinueWithGoogleButton = null;
			}

			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (TogglLogo != null) {
				TogglLogo.Dispose ();
				TogglLogo = null;
			}

			if (TogglmanView != null) {
				TogglmanView.Dispose ();
				TogglmanView = null;
			}
		}
	}
}
