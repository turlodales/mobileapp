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
	[Register ("SsoLoginViewController")]
	partial class SsoLoginViewController
	{
		[Outlet]
		UIKit.UIButton ContinueButton { get; set; }

		[Outlet]
		UIKit.UILabel EmailErrorLabel { get; set; }

		[Outlet]
		Toggl.Daneel.Views.EmailTextField EmailTextField { get; set; }

		[Outlet]
		UIKit.UILabel ErrorLabel { get; set; }

		[Outlet]
		UIKit.UIImageView LogoImageView { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UILabel SsoHeaderLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EmailErrorLabel != null) {
				EmailErrorLabel.Dispose ();
				EmailErrorLabel = null;
			}

			if (EmailTextField != null) {
				EmailTextField.Dispose ();
				EmailTextField = null;
			}

			if (ContinueButton != null) {
				ContinueButton.Dispose ();
				ContinueButton = null;
			}

			if (ErrorLabel != null) {
				ErrorLabel.Dispose ();
				ErrorLabel = null;
			}

			if (LogoImageView != null) {
				LogoImageView.Dispose ();
				LogoImageView = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (SsoHeaderLabel != null) {
				SsoHeaderLabel.Dispose ();
				SsoHeaderLabel = null;
			}
		}
	}
}
