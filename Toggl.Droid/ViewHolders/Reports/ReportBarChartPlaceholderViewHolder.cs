using System;
using System.Collections.Immutable;
using System.Linq;
using Accord.Math.Geometry;
using Accord.Statistics.Kernels;
using Android.App;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Toggl.Core.UI.Extensions;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Extensions;
using Toggl.Droid.Views;
using Toggl.Shared;
using Color = Android.Graphics.Color;

namespace Toggl.Droid.ViewHolders.Reports
{
    public class ReportBarChartPlaceholderViewHolder : ReportElementViewHolder<ReportProjectsBarChartPlaceholderElement>
    {
        private BarChartView barChartView;

        public ReportBarChartPlaceholderViewHolder(View itemView) : base(itemView)
        {
        }

        public ReportBarChartPlaceholderViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            barChartView = ItemView.FindViewById<BarChartView>(Resource.Id.BarChartView);
            var billableLabel = ItemView.FindViewById<TextView>(Resource.Id.BillableText);
            var nonBillableLabel = ItemView.FindViewById<TextView>(Resource.Id.NonBillableText);
            var chartTitle = ItemView.FindViewById<TextView>(Resource.Id.ClockedHours);
            var youHaventTrackedAnythingYetLabel = ItemView.FindViewById<TextView>(Resource.Id.YouHaventTrackedAnythingLabel);
            var getTrackingLabel = ItemView.FindViewById<TextView>(Resource.Id.GetTrackingLabel);

            billableLabel.Text = Resources.Billable;
            nonBillableLabel.Text = Resources.NonBillable;
            chartTitle.Text = Resources.ClockedHours;
            youHaventTrackedAnythingYetLabel.Text = Resources.YouHaventTrackedAnythingYet;
            getTrackingLabel.Text = Resources.GetTracking;
        }

        protected override void UpdateView()
        {
        }
    }
}
