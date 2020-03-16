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
	[Register ("SignUpViewController")]
	partial class SignUpViewController
	{
		[Outlet]
		UIKit.UILabel EmailErrorLabel { get; set; }

		[Outlet]
		Toggl.Daneel.Views.EmailTextField EmailTextField { get; set; }

		[Outlet]
		UIKit.UILabel PasswordErrorLabel { get; set; }

		[Outlet]
		Toggl.iOS.Views.LoginTextField PasswordTextField { get; set; }

		[Outlet]
		UIKit.UIButton ShowPasswordButton { get; set; }

		[Outlet]
		UIKit.UIButton SignUpButton { get; set; }

		[Outlet]
		UIKit.UILabel SignUpErrorLabel { get; set; }

		[Outlet]
		UIKit.UILabel WelcomeLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EmailTextField != null) {
				EmailTextField.Dispose ();
				EmailTextField = null;
			}

			if (PasswordTextField != null) {
				PasswordTextField.Dispose ();
				PasswordTextField = null;
			}

			if (SignUpButton != null) {
				SignUpButton.Dispose ();
				SignUpButton = null;
			}

			if (EmailErrorLabel != null) {
				EmailErrorLabel.Dispose ();
				EmailErrorLabel = null;
			}

			if (PasswordErrorLabel != null) {
				PasswordErrorLabel.Dispose ();
				PasswordErrorLabel = null;
			}

			if (SignUpErrorLabel != null) {
				SignUpErrorLabel.Dispose ();
				SignUpErrorLabel = null;
			}

			if (ShowPasswordButton != null) {
				ShowPasswordButton.Dispose ();
				ShowPasswordButton = null;
			}

			if (WelcomeLabel != null) {
				WelcomeLabel.Dispose ();
				WelcomeLabel = null;
			}
		}
	}
}
