using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Toggl.iOS.Cells.Reports;
using Toggl.iOS.ViewSources;
using UIKit;

namespace Toggl.iOS.Views.Reports
{
    public class ReportsCollectionViewCompactLayout : UICollectionViewLayout
    {
        private const int horizontalCellInset = 16;
        private const int verticalCellInset = 12;
        private const int workspaceButtonInset = 49;

        private List<UICollectionViewLayoutAttributes> layoutAttributes = new List<UICollectionViewLayoutAttributes>();
        private ReportsCollectionViewSource source;
        private bool hasMultipleWorkspaces = false;

        public ReportsCollectionViewCompactLayout(ReportsCollectionViewSource source)
        {
            this.source = source;
        }

        public override CGSize CollectionViewContentSize
        {
            get
            {
                var width = CollectionView.Bounds.Width;
                var height = CollectionView.Bounds.Height;
                if (source.HasDataToDisplay())
                {
                    height = verticalCellInset
                        + ReportsSummaryCollectionViewCell.Height
                        + verticalCellInset * 2
                        + ReportsBarChartCollectionViewCell.Height
                        + verticalCellInset * 2
                        + ReportAdvancedReportsViaWebCollectionViewCell.Height
                        + verticalCellInset * 3
                        + ReportsDonutChartCollectionViewCell.Height
                        + ReportsDonutChartLegendCollectionViewCell.Height * source.NumberOfDonutChartLegendItems()
                        + (hasMultipleWorkspaces ? workspaceButtonInset + verticalCellInset * 2 : 0);
                }
                return new CGSize(width, height);
            }
        }

        public override void PrepareLayout()
        {
            layoutAttributes = new List<UICollectionViewLayoutAttributes>();
            var columnWidth = CollectionViewContentSize.Width - horizontalCellInset * 2;
            for (var i = 0; i < CollectionView.NumberOfItemsInSection(0); i++)
            {
                var indexPath = NSIndexPath.FromItemSection(i, 0);
                UICollectionViewLayoutAttributes attributes = UICollectionViewLayoutAttributes.CreateForCell(indexPath);
                var cellType = source.CellTypeAt(indexPath);
                switch (cellType)
                {
                    case ReportsCollectionViewCell.Summary:
                        attributes.Frame = new CGRect(
                            horizontalCellInset,
                            verticalCellInset,
                            columnWidth,
                            ReportsSummaryCollectionViewCell.Height);
                        break;

                    case ReportsCollectionViewCell.BarChart:
                        attributes.Frame = new CGRect(
                            horizontalCellInset,
                            verticalCellInset
                                + ReportsSummaryCollectionViewCell.Height
                                + verticalCellInset * 2,
                            columnWidth,
                            ReportsBarChartCollectionViewCell.Height);
                        break;

                    case ReportsCollectionViewCell.AdvancedReportsViaWeb:
                        attributes.Frame = new CGRect(
                            horizontalCellInset,
                            verticalCellInset
                                + ReportsSummaryCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportsBarChartCollectionViewCell.Height
                                + verticalCellInset * 2,
                            columnWidth,
                            ReportAdvancedReportsViaWebCollectionViewCell.Height);
                        break;

                    case ReportsCollectionViewCell.DonutChart:
                        attributes.Frame = new CGRect(
                            0,
                            verticalCellInset
                                + ReportsSummaryCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportsBarChartCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportAdvancedReportsViaWebCollectionViewCell.Height
                                + verticalCellInset * 3,
                            CollectionViewContentSize.Width,
                            ReportsDonutChartCollectionViewCell.Height);
                        break;

                    case ReportsCollectionViewCell.DonutChartLegendItem:
                        attributes.Frame = new CGRect(
                            0,
                            verticalCellInset
                                + ReportsSummaryCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportsBarChartCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportAdvancedReportsViaWebCollectionViewCell.Height
                                + verticalCellInset * 3
                                + ReportsDonutChartCollectionViewCell.Height
                                + ReportsDonutChartLegendCollectionViewCell.Height * (indexPath.Item - (CollectionView.NumberOfItemsInSection(0) - source.NumberOfDonutChartLegendItems())),
                            CollectionViewContentSize.Width,
                            ReportsDonutChartLegendCollectionViewCell.Height);
                        break;

                    case ReportsCollectionViewCell.NoData:
                    case ReportsCollectionViewCell.Error:
                        attributes.Frame = new CGRect(
                            0,
                            0,
                            CollectionView.Bounds.Width,
                            CollectionView.Bounds.Height);
                        break;
                }
                layoutAttributes.Add(attributes);
            }
        }

        public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
            => true;

        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
        {
            var visibleAttributes = new List<UICollectionViewLayoutAttributes>();
            foreach (var attributes in layoutAttributes)
            {
                if (attributes.Frame.IntersectsWith(rect))
                    visibleAttributes.Add(attributes);
            }
            return visibleAttributes.ToArray();
        }

        public override UICollectionViewLayoutAttributes LayoutAttributesForItem(NSIndexPath indexPath)
            => layoutAttributes[(int)indexPath.Item];

        public void SetHasMultipleWorkspaces(bool value)
        {
            hasMultipleWorkspaces = value;
            InvalidateLayout();
        }
    }
}
