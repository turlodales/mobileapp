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
	[Register ("TermsAndCountryViewController")]
	partial class TermsAndCountryViewController
	{
		[Outlet]
		UIKit.UIButton BackButton { get; set; }

		[Outlet]
		UIKit.UIButton ConfirmButton { get; set; }

		[Outlet]
		UIKit.UILabel CountryOfResidenceLabel { get; set; }

		[Outlet]
		UIKit.UIButton CountrySelectionButton { get; set; }

		[Outlet]
		UIKit.UIImageView CountrySelectionCaret { get; set; }

		[Outlet]
		UIKit.UIImageView CountrySelectionErrorView { get; set; }

		[Outlet]
		UIKit.UILabel HeaderLabel { get; set; }

		[Outlet]
		UIKit.UITextView TextView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BackButton != null) {
				BackButton.Dispose ();
				BackButton = null;
			}

			if (ConfirmButton != null) {
				ConfirmButton.Dispose ();
				ConfirmButton = null;
			}

			if (CountryOfResidenceLabel != null) {
				CountryOfResidenceLabel.Dispose ();
				CountryOfResidenceLabel = null;
			}

			if (CountrySelectionButton != null) {
				CountrySelectionButton.Dispose ();
				CountrySelectionButton = null;
			}

			if (CountrySelectionCaret != null) {
				CountrySelectionCaret.Dispose ();
				CountrySelectionCaret = null;
			}

			if (CountrySelectionErrorView != null) {
				CountrySelectionErrorView.Dispose ();
				CountrySelectionErrorView = null;
			}

			if (HeaderLabel != null) {
				HeaderLabel.Dispose ();
				HeaderLabel = null;
			}

			if (TextView != null) {
				TextView.Dispose ();
				TextView = null;
			}
		}
	}
}
