using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using Toggl.Core.UI.ViewModels.Reports;
using Toggl.iOS.Cells.Reports;
using UIKit;

namespace Toggl.iOS.ViewSources
{
    public enum ReportsCollectionViewCell
    {
        Summary,
        BarChart,
        AdvancedReportsViaWeb,
        DonutChart,
        DonutChartLegendItem,
        Error,
        NoData
    }

    public class ReportsCollectionViewSource : UICollectionViewSource
    {
        private readonly UICollectionView collectionView;

        private IImmutableList<IReportElement> elements;

        private const string summaryCellIdentifier = nameof(summaryCellIdentifier);
        private const string barChartCellIdentifier = nameof(barChartCellIdentifier);
        private const string donutChartCellIdentifier = nameof(donutChartCellIdentifier);
        private const string donutChartLegendCellIdentifier = nameof(donutChartLegendCellIdentifier);
        private const string noDataCellIdentifier = nameof(noDataCellIdentifier);
        private const string errorCellIdentifier = nameof(errorCellIdentifier);
        private const string workspaceCellIdentifier = nameof(workspaceCellIdentifier);
        private const string advancedReportsViaWebCellIdentifier = nameof(advancedReportsViaWebCellIdentifier);

        private ISubject<ReportsCollectionViewCell> itemTappedSubject = new Subject<ReportsCollectionViewCell>();
        public IObservable<ReportsCollectionViewCell> ItemTapped { get; }

        public ReportsCollectionViewSource(UICollectionView collectionView)
        {
            this.collectionView = collectionView;

            ItemTapped = itemTappedSubject.AsObservable();

            collectionView.RegisterNibForCell(ReportsSummaryCollectionViewCell.Nib, summaryCellIdentifier);
            collectionView.RegisterNibForCell(ReportsBarChartCollectionViewCell.Nib, barChartCellIdentifier);
            collectionView.RegisterNibForCell(ReportsDonutChartCollectionViewCell.Nib, donutChartCellIdentifier);
            collectionView.RegisterNibForCell(ReportsDonutChartLegendCollectionViewCell.Nib, donutChartLegendCellIdentifier);
            collectionView.RegisterNibForCell(ReportsNoDataCollectionViewCell.Nib, noDataCellIdentifier);
            collectionView.RegisterNibForCell(ReportsErrorCollectionViewCell.Nib, errorCellIdentifier);
            collectionView.RegisterNibForCell(ReportAdvancedReportsViaWebCollectionViewCell.Nib, advancedReportsViaWebCellIdentifier);
        }

        public void SetNewElements(IImmutableList<IReportElement> elements)
        {
            this.elements = elements.Where(e => e.GetType().Name != nameof(ReportWorkspaceNameElement)).ToImmutableList();
            collectionView.ReloadData();
            collectionView.CollectionViewLayout.InvalidateLayout();
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            itemTappedSubject.OnNext(CellTypeAt(indexPath));
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            switch (elements[(int)indexPath.Item])
            {
                case ReportSummaryElement element:
                    var summaryCell = collectionView.DequeueReusableCell(summaryCellIdentifier, indexPath) as ReportsSummaryCollectionViewCell;
                    summaryCell.SetElement(element);
                    return summaryCell;
                case ReportBarChartElement element:
                    var barChartCell = collectionView.DequeueReusableCell(barChartCellIdentifier, indexPath) as ReportsBarChartCollectionViewCell;
                    barChartCell.SetElement(element);
                    return barChartCell;
                case ReportDonutChartDonutElement element:
                    var donutCell = collectionView.DequeueReusableCell(donutChartCellIdentifier, indexPath) as ReportsDonutChartCollectionViewCell;
                    donutCell.SetElement(element, indexPath.Item == elements.Count - 1);
                    return donutCell;
                case ReportProjectsDonutChartLegendItemElement element:
                    var donutLegendItemCell = collectionView.DequeueReusableCell(donutChartLegendCellIdentifier, indexPath) as ReportsDonutChartLegendCollectionViewCell;
                    donutLegendItemCell.SetElement(element, indexPath.Item == elements.Count - 1);
                    return donutLegendItemCell;
                case ReportNoDataElement _:
                    var noDataCell = collectionView.DequeueReusableCell(noDataCellIdentifier, indexPath) as ReportsNoDataCollectionViewCell;
                    return noDataCell;
                case ReportErrorElement element:
                    var errorCell = collectionView.DequeueReusableCell(errorCellIdentifier, indexPath) as ReportsErrorCollectionViewCell;
                    errorCell.setElement(element);
                    return errorCell;
                case ReportAdvancedReportsViaWebElement element:
                    var advancedReportsViaWebCell = collectionView.DequeueReusableCell(advancedReportsViaWebCellIdentifier, indexPath) as ReportAdvancedReportsViaWebCollectionViewCell;
                    advancedReportsViaWebCell.SetElement(element);
                    return advancedReportsViaWebCell;
                default:
                    var defaultCell = collectionView.DequeueReusableCell(errorCellIdentifier, indexPath) as ReportsErrorCollectionViewCell;
                    defaultCell.setElement(new ReportErrorElement(new ArgumentException()));
                    return defaultCell;
            }
        }

        public override nint NumberOfSections(UICollectionView collectionView)
            => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => elements == null ? 0 : elements.Count;

        public ReportsCollectionViewCell CellTypeAt(NSIndexPath indexPath)
        {
            switch (elements[(int)indexPath.Item])
            {
                case ReportSummaryElement _:
                    return ReportsCollectionViewCell.Summary;
                case ReportBarChartElement _:
                    return ReportsCollectionViewCell.BarChart;
                case ReportDonutChartDonutElement _:
                    return ReportsCollectionViewCell.DonutChart;
                case ReportDonutChartLegendItemElement _:
                    return ReportsCollectionViewCell.DonutChartLegendItem;
                case ReportNoDataElement _:
                    return ReportsCollectionViewCell.NoData;
                case ReportAdvancedReportsViaWebElement _:
                    return ReportsCollectionViewCell.AdvancedReportsViaWeb;
                default:
                    return ReportsCollectionViewCell.Error;
            }
        }

        public int NumberOfDonutChartLegendItems()
        {
            var num = elements == null
                ? 0
                : elements.Where(e => e is ReportDonutChartLegendItemElement _).Count();
            return num;
        }

        public bool HasDataToDisplay()
            => elements != null && elements.Count > 0;
    }
}
