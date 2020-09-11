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
	[Register ("ReportsViewController")]
	partial class ReportsViewController
	{
		[Outlet]
		UIKit.UIView ChangeDateRangeTooltip { get; set; }

		[Outlet]
		Toggl.iOS.Views.TriangleView ChangeDateRangeTooltipArrow { get; set; }

		[Outlet]
		UIKit.UIView ChangeDateRangeTooltipBackground { get; set; }

		[Outlet]
		UIKit.UIImageView ChangeDateRangeTooltipCloseIcone { get; set; }

		[Outlet]
		UIKit.UILabel ChangeDateRangeTooltipGotItLabel { get; set; }

		[Outlet]
		UIKit.UILabel ChangeDateRangeTooltipMessageLabel { get; set; }

		[Outlet]
		UIKit.UILabel ChangeDateRangeTooltipTitleLabel { get; set; }

		[Outlet]
		UIKit.UICollectionView CollectionView { get; set; }

		[Outlet]
		UIKit.UIView WorkspaceButton { get; set; }

		[Outlet]
		Toggl.iOS.Views.FadeView WorkspaceFadeView { get; set; }

		[Outlet]
		UIKit.UILabel WorkspaceLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CollectionView != null) {
				CollectionView.Dispose ();
				CollectionView = null;
			}

			if (WorkspaceButton != null) {
				WorkspaceButton.Dispose ();
				WorkspaceButton = null;
			}

			if (WorkspaceFadeView != null) {
				WorkspaceFadeView.Dispose ();
				WorkspaceFadeView = null;
			}

			if (WorkspaceLabel != null) {
				WorkspaceLabel.Dispose ();
				WorkspaceLabel = null;
			}

			if (ChangeDateRangeTooltip != null) {
				ChangeDateRangeTooltip.Dispose ();
				ChangeDateRangeTooltip = null;
			}

			if (ChangeDateRangeTooltipBackground != null) {
				ChangeDateRangeTooltipBackground.Dispose ();
				ChangeDateRangeTooltipBackground = null;
			}

			if (ChangeDateRangeTooltipTitleLabel != null) {
				ChangeDateRangeTooltipTitleLabel.Dispose ();
				ChangeDateRangeTooltipTitleLabel = null;
			}

			if (ChangeDateRangeTooltipMessageLabel != null) {
				ChangeDateRangeTooltipMessageLabel.Dispose ();
				ChangeDateRangeTooltipMessageLabel = null;
			}

			if (ChangeDateRangeTooltipGotItLabel != null) {
				ChangeDateRangeTooltipGotItLabel.Dispose ();
				ChangeDateRangeTooltipGotItLabel = null;
			}

			if (ChangeDateRangeTooltipCloseIcone != null) {
				ChangeDateRangeTooltipCloseIcone.Dispose ();
				ChangeDateRangeTooltipCloseIcone = null;
			}

			if (ChangeDateRangeTooltipArrow != null) {
				ChangeDateRangeTooltipArrow.Dispose ();
				ChangeDateRangeTooltipArrow = null;
			}
		}
	}
}
