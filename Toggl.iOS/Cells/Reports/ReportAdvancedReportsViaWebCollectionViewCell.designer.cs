// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.iOS.Cells.Reports
{
	[Register ("ReportAdvancedReportsViaWebCollectionViewCell")]
	partial class ReportAdvancedReportsViaWebCollectionViewCell
	{
		[Outlet]
		UIKit.UIImageView ArrowImageView { get; set; }

		[Outlet]
		UIKit.UILabel ButtonLabel { get; set; }

		[Outlet]
		UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ArrowImageView != null) {
				ArrowImageView.Dispose ();
				ArrowImageView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (ButtonLabel != null) {
				ButtonLabel.Dispose ();
				ButtonLabel = null;
			}
		}
	}
}
