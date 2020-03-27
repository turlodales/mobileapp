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
	[Register ("ForgotPasswordViewController")]
	partial class ForgotPasswordViewController
	{
		[Outlet]
		Toggl.iOS.Views.ActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIView DoneCard { get; set; }

		[Outlet]
		Toggl.iOS.Views.LoginTextField EmailTextField { get; set; }

		[Outlet]
		UIKit.UILabel ErrorLabel { get; set; }

		[Outlet]
		UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		UIKit.UIButton ResetPasswordButton { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UILabel SuccessMessageLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (DoneCard != null) {
				DoneCard.Dispose ();
				DoneCard = null;
			}

			if (EmailTextField != null) {
				EmailTextField.Dispose ();
				EmailTextField = null;
			}

			if (ErrorLabel != null) {
				ErrorLabel.Dispose ();
				ErrorLabel = null;
			}

			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (ResetPasswordButton != null) {
				ResetPasswordButton.Dispose ();
				ResetPasswordButton = null;
			}

			if (SuccessMessageLabel != null) {
				SuccessMessageLabel.Dispose ();
				SuccessMessageLabel = null;
			}
		}
	}
}
