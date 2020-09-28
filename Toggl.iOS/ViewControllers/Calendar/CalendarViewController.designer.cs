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
	[Register ("CalendarViewController")]
	partial class CalendarViewController
	{
		[Outlet]
		Toggl.iOS.Views.AccessibilityAwareView CurrentTimeEntryCard { get; set; }

		[Outlet]
		UIKit.UILabel CurrentTimeEntryDescriptionLabel { get; set; }

		[Outlet]
		UIKit.UILabel CurrentTimeEntryElapsedTimeLabel { get; set; }

		[Outlet]
		UIKit.UILabel CurrentTimeEntryProjectTaskClientLabel { get; set; }

		[Outlet]
		UIKit.UILabel DailyTrackedTimeLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint DailyTrackedTimeLeadingConstraint { get; set; }

		[Outlet]
		UIKit.UIView DayViewContainer { get; set; }

		[Outlet]
		Toggl.iOS.Views.FadeView RunningEntryDescriptionFadeView { get; set; }

		[Outlet]
		UIKit.UILabel SelectedDateLabel { get; set; }

		[Outlet]
		UIKit.UIButton SettingsButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint SettingsButtonTrailingConstraint { get; set; }

		[Outlet]
		UIKit.UIView StartTimeEntryButton { get; set; }

		[Outlet]
		UIKit.UIImageView StartTimeEntryButtonIcon { get; set; }

		[Outlet]
		UIKit.UIButton StopTimeEntryButton { get; set; }

		[Outlet]
		UIKit.UICollectionView WeekViewCollectionView { get; set; }

		[Outlet]
		UIKit.UIView WeekViewContainer { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint WeekViewContainerWidthConstraint { get; set; }

		[Outlet]
		UIKit.UIView WeekViewDayHeaderContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CurrentTimeEntryCard != null) {
				CurrentTimeEntryCard.Dispose ();
				CurrentTimeEntryCard = null;
			}

			if (CurrentTimeEntryDescriptionLabel != null) {
				CurrentTimeEntryDescriptionLabel.Dispose ();
				CurrentTimeEntryDescriptionLabel = null;
			}

			if (CurrentTimeEntryElapsedTimeLabel != null) {
				CurrentTimeEntryElapsedTimeLabel.Dispose ();
				CurrentTimeEntryElapsedTimeLabel = null;
			}

			if (CurrentTimeEntryProjectTaskClientLabel != null) {
				CurrentTimeEntryProjectTaskClientLabel.Dispose ();
				CurrentTimeEntryProjectTaskClientLabel = null;
			}

			if (DailyTrackedTimeLabel != null) {
				DailyTrackedTimeLabel.Dispose ();
				DailyTrackedTimeLabel = null;
			}

			if (DailyTrackedTimeLeadingConstraint != null) {
				DailyTrackedTimeLeadingConstraint.Dispose ();
				DailyTrackedTimeLeadingConstraint = null;
			}

			if (DayViewContainer != null) {
				DayViewContainer.Dispose ();
				DayViewContainer = null;
			}

			if (RunningEntryDescriptionFadeView != null) {
				RunningEntryDescriptionFadeView.Dispose ();
				RunningEntryDescriptionFadeView = null;
			}

			if (SelectedDateLabel != null) {
				SelectedDateLabel.Dispose ();
				SelectedDateLabel = null;
			}

			if (SettingsButton != null) {
				SettingsButton.Dispose ();
				SettingsButton = null;
			}

			if (SettingsButtonTrailingConstraint != null) {
				SettingsButtonTrailingConstraint.Dispose ();
				SettingsButtonTrailingConstraint = null;
			}

			if (StartTimeEntryButton != null) {
				StartTimeEntryButton.Dispose ();
				StartTimeEntryButton = null;
			}

			if (StartTimeEntryButtonIcon != null) {
				StartTimeEntryButtonIcon.Dispose ();
				StartTimeEntryButtonIcon = null;
			}

			if (StopTimeEntryButton != null) {
				StopTimeEntryButton.Dispose ();
				StopTimeEntryButton = null;
			}

			if (WeekViewCollectionView != null) {
				WeekViewCollectionView.Dispose ();
				WeekViewCollectionView = null;
			}

			if (WeekViewContainer != null) {
				WeekViewContainer.Dispose ();
				WeekViewContainer = null;
			}

			if (WeekViewContainerWidthConstraint != null) {
				WeekViewContainerWidthConstraint.Dispose ();
				WeekViewContainerWidthConstraint = null;
			}

			if (WeekViewDayHeaderContainer != null) {
				WeekViewDayHeaderContainer.Dispose ();
				WeekViewDayHeaderContainer = null;
			}
		}
	}
}
