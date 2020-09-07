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
	[Register ("ReportsBarChartPlaceholderCollectionViewCell")]
	partial class ReportsBarChartPlaceholderCollectionViewCell
	{
		[Outlet]
		UIKit.UILabel GetTrackingLabel { get; set; }

		[Outlet]
		UIKit.UILabel YouHaventTrackedAnythingLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (YouHaventTrackedAnythingLabel != null) {
				YouHaventTrackedAnythingLabel.Dispose ();
				YouHaventTrackedAnythingLabel = null;
			}

			if (GetTrackingLabel != null) {
				GetTrackingLabel.Dispose ();
				GetTrackingLabel = null;
			}
		}
	}
}
