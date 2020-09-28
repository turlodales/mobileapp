using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Shared;

namespace Toggl.Droid.ViewHolders.Reports
{
    public class ReportDonutChartPlaceholderViewHolder : ReportElementViewHolder<ReportDonutChartPlaceholderIllustrationElement>
    {
        public ReportDonutChartPlaceholderViewHolder(View itemView) : base(itemView)
        {
        }

        public ReportDonutChartPlaceholderViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            var messageLabel = ItemView.FindViewById<TextView>(Resource.Id.MessageLabel);
            messageLabel.Text = Resources.DonutChartPlaceholderMessage;
        }

        protected override void UpdateView()
        {
        }
    }
}
