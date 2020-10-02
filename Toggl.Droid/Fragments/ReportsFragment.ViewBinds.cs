using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Toggl.Droid.Views;

namespace Toggl.Droid.Fragments
{
    public sealed partial class ReportsFragment
    {
        private FloatingActionButton selectWorkspaceFab;
        private TextView toolbarCurrentDateRangeText;
        private RecyclerView reportsRecyclerView;
        private TooltipLayout changeDateRangeTooltip;
        private AppBarLayout appBarLayout;

        protected override void InitializeViews(View fragmentView)
        {
            selectWorkspaceFab = fragmentView.FindViewById<FloatingActionButton>(Resource.Id.SelectWorkspaceFAB);
            toolbarCurrentDateRangeText = fragmentView.FindViewById<TextView>(Resource.Id.ToolbarCurrentDateRangeText);
            changeDateRangeTooltip = fragmentView.FindViewById<TooltipLayout>(Resource.Id.ChangeDateRangeTooltip);
            reportsRecyclerView = fragmentView.FindViewById<RecyclerView>(Resource.Id.ReportsFragmentRecyclerView);
            appBarLayout = fragmentView.FindViewById<AppBarLayout>(Resource.Id.AppBarLayout);

            changeDateRangeTooltip.Title = Shared.Resources.ChangeDateRangeTootlipTitle;
            changeDateRangeTooltip.Text = Shared.Resources.ChangeDateRangeTooltipMessage;
            changeDateRangeTooltip.ButtonText = Shared.Resources.OkGotIt;
        }
    }
}
