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
	[Register ("StartTimeEntryViewController")]
	partial class StartTimeEntryViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton BillableButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint BillableButtonWidthConstraint { get; set; }

		[Outlet]
		UIKit.UIView BillableTooltip { get; set; }

		[Outlet]
		Toggl.iOS.Views.TriangleView BillableTooltipArrow { get; set; }

		[Outlet]
		UIKit.UIView BillableTooltipBackground { get; set; }

		[Outlet]
		UIKit.UIButton BillableTooltipDetailsButton { get; set; }

		[Outlet]
		UIKit.UILabel BillableTooltipMessageLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint BottomDistanceConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView BottomOptionsSheet { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton CloseButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton DateTimeButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton DoneButton { get; set; }

		[Outlet]
		Toggl.iOS.Views.AutocompleteTextViewPlaceholder Placeholder { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton ProjectsButton { get; set; }

		[Outlet]
		UIKit.UIView ProjectsTooltip { get; set; }

		[Outlet]
		Toggl.iOS.Views.TriangleView ProjectsTooltipArrow { get; set; }

		[Outlet]
		UIKit.UIView ProjectsTooltipBackground { get; set; }

		[Outlet]
		UIKit.UIImageView ProjectsTooltipCloseIcon { get; set; }

		[Outlet]
		UIKit.UILabel ProjectsTooltipLabel { get; set; }

		[Outlet]
		UIKit.UIButton StartDateButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITableView SuggestionsTableView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton TagsButton { get; set; }

		[Outlet]
		Toggl.iOS.Views.EditDuration.DurationField TimeInput { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TimeInputTrailingConstraint { get; set; }

		[Outlet]
		UIKit.UILabel TimeLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint TimeLabelTrailingConstraint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BillableButton != null) {
				BillableButton.Dispose ();
				BillableButton = null;
			}

			if (BillableButtonWidthConstraint != null) {
				BillableButtonWidthConstraint.Dispose ();
				BillableButtonWidthConstraint = null;
			}

			if (BillableTooltip != null) {
				BillableTooltip.Dispose ();
				BillableTooltip = null;
			}

			if (BillableTooltipArrow != null) {
				BillableTooltipArrow.Dispose ();
				BillableTooltipArrow = null;
			}

			if (BillableTooltipBackground != null) {
				BillableTooltipBackground.Dispose ();
				BillableTooltipBackground = null;
			}

			if (BillableTooltipDetailsButton != null) {
				BillableTooltipDetailsButton.Dispose ();
				BillableTooltipDetailsButton = null;
			}

			if (BillableTooltipMessageLabel != null) {
				BillableTooltipMessageLabel.Dispose ();
				BillableTooltipMessageLabel = null;
			}

			if (BottomDistanceConstraint != null) {
				BottomDistanceConstraint.Dispose ();
				BottomDistanceConstraint = null;
			}

			if (BottomOptionsSheet != null) {
				BottomOptionsSheet.Dispose ();
				BottomOptionsSheet = null;
			}

			if (CloseButton != null) {
				CloseButton.Dispose ();
				CloseButton = null;
			}

			if (DateTimeButton != null) {
				DateTimeButton.Dispose ();
				DateTimeButton = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (DoneButton != null) {
				DoneButton.Dispose ();
				DoneButton = null;
			}

			if (Placeholder != null) {
				Placeholder.Dispose ();
				Placeholder = null;
			}

			if (ProjectsButton != null) {
				ProjectsButton.Dispose ();
				ProjectsButton = null;
			}

			if (ProjectsTooltip != null) {
				ProjectsTooltip.Dispose ();
				ProjectsTooltip = null;
			}

			if (ProjectsTooltipArrow != null) {
				ProjectsTooltipArrow.Dispose ();
				ProjectsTooltipArrow = null;
			}

			if (ProjectsTooltipBackground != null) {
				ProjectsTooltipBackground.Dispose ();
				ProjectsTooltipBackground = null;
			}

			if (ProjectsTooltipCloseIcon != null) {
				ProjectsTooltipCloseIcon.Dispose ();
				ProjectsTooltipCloseIcon = null;
			}

			if (ProjectsTooltipLabel != null) {
				ProjectsTooltipLabel.Dispose ();
				ProjectsTooltipLabel = null;
			}

			if (StartDateButton != null) {
				StartDateButton.Dispose ();
				StartDateButton = null;
			}

			if (SuggestionsTableView != null) {
				SuggestionsTableView.Dispose ();
				SuggestionsTableView = null;
			}

			if (TagsButton != null) {
				TagsButton.Dispose ();
				TagsButton = null;
			}

			if (TimeInput != null) {
				TimeInput.Dispose ();
				TimeInput = null;
			}

			if (TimeInputTrailingConstraint != null) {
				TimeInputTrailingConstraint.Dispose ();
				TimeInputTrailingConstraint = null;
			}

			if (TimeLabel != null) {
				TimeLabel.Dispose ();
				TimeLabel = null;
			}

			if (TimeLabelTrailingConstraint != null) {
				TimeLabelTrailingConstraint.Dispose ();
				TimeLabelTrailingConstraint = null;
			}
		}
	}
}
