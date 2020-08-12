using Android.Runtime;
using Android.Views;
using System;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.Droid.Adapters;
using Toggl.Droid.ViewHolders;
using Toggl.Droid.ViewHolders.Reports;
using Toggl.Shared.Extensions;
using static Toggl.Droid.Fragments.ReportsAdapter.ViewType;

namespace Toggl.Droid.Fragments
{
    public class ReportsAdapter : BaseRecyclerAdapter<IReportElement>
    {
        public enum ViewType
        {
            WorkspaceName,
            Summary,
            BarChart,
            Donut,
            DonutLegendItem,
            NoData,
            Error,
            AdvancedReportsOnWeb
        }

        private readonly ViewAction openYourPlanAction;

        public ReportsAdapter(ViewAction openYourPlanAction)
        {
            this.openYourPlanAction = openYourPlanAction;
        }

        public ReportsAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int GetItemViewType(int position)
        {
            var itemType = Items[position] switch
            {
                ReportWorkspaceNameElement _ => WorkspaceName,
                ReportSummaryElement _ => Summary,
                ReportBarChartElement _ => BarChart,
                ReportDonutChartDonutElement _ => Donut,
                ReportDonutChartLegendItemElement _ => DonutLegendItem,
                ReportNoDataElement _ => NoData,
                ReportErrorElement _ => Error,
                ReportAdvancedReportsViaWebElement _ => AdvancedReportsOnWeb,
                _ => throw new InvalidOperationException($"Invalid Report Segment View Type for position {position}."),
            };

            return (int)itemType;
        }

        protected override BaseRecyclerViewHolder<IReportElement> CreateViewHolder(ViewGroup parent, LayoutInflater inflater, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case WorkspaceName:
                    var workpaceNameCell = inflater.Inflate(Resource.Layout.ReportWorkspaceNameElement, parent, false);
                    return new ReportWorkspaceNameViewHolder(workpaceNameCell);

                case Summary:
                    var summaryCell = inflater.Inflate(Resource.Layout.ReportSummaryElement, parent, false);
                    return new ReportSummaryViewHolder(summaryCell);

                case BarChart:
                    var barChartCell = inflater.Inflate(Resource.Layout.ReportsBarChartElement, parent, false);
                    return new ReportBarChartViewHolder(barChartCell);

                case Donut:
                    var donutCell = inflater.Inflate(Resource.Layout.ReportDonutChartDonutElement, parent, false);
                    return new ReportDonutChartDonutViewHolder(donutCell);

                case DonutLegendItem:
                    var donutLegendItemCell = inflater.Inflate(Resource.Layout.ReportDonutLegendItem, parent, false);
                    return new ReportDonutChartLegendItemViewHolder(donutLegendItemCell);

                case NoData:
                    var noDataCell = inflater.Inflate(Resource.Layout.ReportNoDataElement, parent, false);
                    return new ReportNoDataElementViewHolder(noDataCell);

                case Error:
                    var errorCell = inflater.Inflate(Resource.Layout.ReportErrorElement, parent, false);
                    return new ReportErrorElementViewHolder(errorCell);

                case AdvancedReportsOnWeb:
                    var advancedReportsCell = inflater.Inflate(Resource.Layout.ReportAdvancedOnWebCard, parent, false);
                    return new ReportAdvancedCardOnWebViewHolder(advancedReportsCell, openYourPlanAction);

                default:
                    throw new InvalidOperationException("Can't create a viewholder for the given ViewType.");
            }
        }
    }
}
