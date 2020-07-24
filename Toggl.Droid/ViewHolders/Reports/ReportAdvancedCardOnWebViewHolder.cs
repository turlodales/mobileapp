using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Extensions;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.ViewHolders.Reports
{
    public class ReportAdvancedCardOnWebViewHolder : ReportElementViewHolder<ReportAdvancedReportsViaWebElement>
    {
        private TextView yourPlan;
        private readonly ViewAction action;

        public ReportAdvancedCardOnWebViewHolder(View itemView, ViewAction action)
            : base(itemView)
        {
            this.action = action;
        }

        public ReportAdvancedCardOnWebViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            ItemView.FindViewById<TextView>(Resource.Id.Title).Text = Resources.AdvancedReportsViaWeb;
            ItemView.FindViewById<TextView>(Resource.Id.Description).Text = Resources.AdvancedReportsFeatures;

            yourPlan = ItemView.FindViewById<TextView>(Resource.Id.YouPlanLink);
            yourPlan.Text = Resources.AvailableOnOtherPlans + " >";
            yourPlan.Click += delegate { action.Execute(); };
        }

        protected override void UpdateView()
        {
            yourPlan.Visibility = Item.ShouldShowAvailableOnOtherPlans.ToVisibility();
        }
    }
}
