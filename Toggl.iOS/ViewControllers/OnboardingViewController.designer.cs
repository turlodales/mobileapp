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
		UIKit.UIPageControl PageControl { get; set; }

		[Outlet]
		UIKit.UIImageView TogglLogo { get; set; }
		
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

			if (PageControl != null) {
				PageControl.Dispose ();
				PageControl = null;
			}

			if (TogglLogo != null) {
				TogglLogo.Dispose ();
				TogglLogo = null;
			}
		}
	}
}
