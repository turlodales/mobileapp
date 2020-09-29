using Android.OS;
using Android.Views;
using System;
using System.Reactive.Linq;
using Google.Android.Material.BottomNavigation;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Extensions;
using Toggl.Droid.Extensions.Reactive;
using Toggl.Droid.LayoutManagers;
using Toggl.Droid.Presentation;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Fragments
{
    public sealed partial class ReportsFragment : ReactiveTabFragment<ReportsViewModel>, IScrollableToStart
    {
        private ReportsAdapter adapter;

        public BottomNavigationView BottomNavigationView { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ReportsFragment, container, false);
            InitializeViews(view);

            SetupToolbar(view);
            setupRecyclerView();

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            selectWorkspaceFab.Rx().Tap()
                .Subscribe(ViewModel.SelectWorkspace.Inputs)
                .DisposedBy(DisposeBag);

            ViewModel.FormattedDateRange
                .Subscribe(toolbarCurrentDateRangeText.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            ViewModel.Elements
                .Subscribe(adapter.Rx().Items())
                .DisposedBy(DisposeBag);

            toolbarCurrentDateRangeText.Rx()
                .BindAction(ViewModel.SelectDateRange)
                .DisposedBy(DisposeBag);

            ViewModel.ChangeDateRangeTooltipShouldBeVisible
                .Subscribe(changeDateRangeTooltip.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            changeDateRangeTooltip.Rx().Tap()
                .Subscribe(ViewModel.ChangeDateRangeTooltipTapped.Inputs)
                .DisposedBy(DisposeBag);
        }

        public void ScrollToStart()
        {
            reportsRecyclerView?.SmoothScrollToPosition(0);
        }

        private void setupRecyclerView()
        {
            adapter = new ReportsAdapter(ViewModel.OpenYourPlanView);
            reportsRecyclerView.AttachMaterialScrollBehaviour(appBarLayout);
            reportsRecyclerView.SetLayoutManager(new UnpredictiveLinearLayoutManager(Context));
            reportsRecyclerView.SetAdapter(adapter);

            if (BottomNavigationView != null)
            {
                adapter.ItemTapObservable
                    .Where(item => item is ReportProjectsBarChartPlaceholderElement)
                    .Subscribe(_ => BottomNavigationView.SelectedItemId = Resource.Id.MainTabTimerItem)
                    .DisposedBy(DisposeBag);
            }
        }
    }
}
