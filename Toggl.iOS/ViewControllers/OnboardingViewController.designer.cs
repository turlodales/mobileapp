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
		UIKit.NSLayoutConstraint ButtonsStackBottomConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ButtonsStackTopConstraint { get; set; }

		[Outlet]
		UIKit.UIStackView ButtonsStackView { get; set; }

		[Outlet]
		UIKit.UIButton CancelSsoButton { get; set; }

		[Outlet]
		UIKit.UIButton ContinueWithEmailButton { get; set; }

		[Outlet]
		UIKit.UIButton ContinueWithGoogleButton { get; set; }

		[Outlet]
		UIKit.UILabel LoginToEnableSsoLabel { get; set; }

		[Outlet]
		UIKit.UIButton LoginWithSsoButton { get; set; }

		[Outlet]
		UIKit.UIPageControl PageControl { get; set; }

		[Outlet]
		UIKit.UIImageView TogglLogo { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ButtonsStackView != null) {
				ButtonsStackView.Dispose ();
				ButtonsStackView = null;
			}

			if (CancelSsoButton != null) {
				CancelSsoButton.Dispose ();
				CancelSsoButton = null;
			}

			if (ContinueWithEmailButton != null) {
				ContinueWithEmailButton.Dispose ();
				ContinueWithEmailButton = null;
			}

			if (ContinueWithGoogleButton != null) {
				ContinueWithGoogleButton.Dispose ();
				ContinueWithGoogleButton = null;
			}

			if (LoginToEnableSsoLabel != null) {
				LoginToEnableSsoLabel.Dispose ();
				LoginToEnableSsoLabel = null;
			}

			if (LoginWithSsoButton != null) {
				LoginWithSsoButton.Dispose ();
				LoginWithSsoButton = null;
			}

			if (PageControl != null) {
				PageControl.Dispose ();
				PageControl = null;
			}

			if (TogglLogo != null) {
				TogglLogo.Dispose ();
				TogglLogo = null;
			}

			if (ButtonsStackBottomConstraint != null) {
				ButtonsStackBottomConstraint.Dispose ();
				ButtonsStackBottomConstraint = null;
			}

			if (ButtonsStackTopConstraint != null) {
				ButtonsStackTopConstraint.Dispose ();
				ButtonsStackTopConstraint = null;
			}
		}
	}
}
