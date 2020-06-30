using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Extensions;
using Toggl.Droid.Helper;

namespace Toggl.Droid.ViewHolders.Reports
{
    public class ReportDonutChartLegendItemViewHolder : ReportElementViewHolder<ReportProjectsDonutChartLegendItemElement>
    {
        private TextView projectName;
        private TextView clientName;
        private TextView duration;
        private TextView percentage;

        public ReportDonutChartLegendItemViewHolder(View itemView) : base(itemView)
        {
        }

        public ReportDonutChartLegendItemViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            projectName = ItemView.FindViewById<TextView>(Resource.Id.ReportsFragmentItemProjectName);
            clientName = ItemView.FindViewById<TextView>(Resource.Id.ReportsFragmentItemClientName);
            duration = ItemView.FindViewById<TextView>(Resource.Id.ReportsFragmentItemDuration);
            percentage = ItemView.FindViewById<TextView>(Resource.Id.ReportsFragmentItemPercentage);
        }

        protected override void UpdateView()
        {
            projectName.Text = Item.Name;
            projectName.SetTextColor(Shared.Color.ParseAndAdjustToLabel(
                Item.Color, ActiveTheme.Is.DarkTheme
            ).ToNativeColor());
            clientName.Text = Item.Client;
            duration.Text = Item.Value;
            percentage.Text = $"{Item.Percentage:0.00}%";
        }
    }
}
