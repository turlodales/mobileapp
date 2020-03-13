// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.iOS
{
	[Register ("OnboardingStepView")]
	partial class OnboardingPageView
	{
		[Outlet]
		UIKit.UIView ContentView { get; set; }

		[Outlet]
		UIKit.UILabel MessageLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (ContentView != null) {
				ContentView.Dispose ();
				ContentView = null;
			}
		}
	}
}
