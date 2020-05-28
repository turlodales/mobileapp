using Foundation;
using ObjCRuntime;
using System;
using Toggl.iOS.Extensions;
using Toggl.Shared;
using UIKit;

namespace Toggl.iOS.Views
{
    public sealed partial class TimeEntriesEmptyLogView : UIView
    {
        public TimeEntriesEmptyLogView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            GetReadyLabel.Text = Resources.GetReadyToTrackTimeAndBoostYourProductivity;
        }

        public static TimeEntriesEmptyLogView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(TimeEntriesEmptyLogView), null, null);
            return Runtime.GetNSObject<TimeEntriesEmptyLogView>(arr.ValueAt(0));
        }
    }
}
