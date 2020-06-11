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
	[Register ("SsoLinkViewController")]
	partial class SsoLinkViewController
	{
		[Outlet]
		UIKit.UIButton LinkButton { get; set; }

		[Outlet]
		UIKit.UIImageView LogoImageView { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UILabel SsoLinkDescriptionLabel { get; set; }

		[Outlet]
		UIKit.UILabel SsoLinkHeaderLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SsoLinkHeaderLabel != null) {
				SsoLinkHeaderLabel.Dispose ();
				SsoLinkHeaderLabel = null;
			}

			if (SsoLinkDescriptionLabel != null) {
				SsoLinkDescriptionLabel.Dispose ();
				SsoLinkDescriptionLabel = null;
			}

			if (LinkButton != null) {
				LinkButton.Dispose ();
				LinkButton = null;
			}

			if (LogoImageView != null) {
				LogoImageView.Dispose ();
				LogoImageView = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}
		}
	}
}
