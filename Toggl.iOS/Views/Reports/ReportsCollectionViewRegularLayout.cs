using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Toggl.iOS.Cells.Reports;
using Toggl.iOS.ViewSources;
using UIKit;

namespace Toggl.iOS.Views.Reports
{
    public class ReportsCollectionViewRegularLayout : UICollectionViewLayout
    {
        private const int maxWidth = 834;
        private const int horizontalCellInset = 8;
        private const int verticalCellInset = 12;

        private List<UICollectionViewLayoutAttributes> layoutAttributes = new List<UICollectionViewLayoutAttributes>();
        private ReportsCollectionViewSource source;

        public ReportsCollectionViewRegularLayout(ReportsCollectionViewSource source)
        {
            this.source = source;
        }

        public override CGSize CollectionViewContentSize
        {
            get
            {
                var width = CollectionView.Bounds.Width;
                if (width > maxWidth)
                    width = maxWidth;
                var height = CollectionView.Bounds.Height;
                if (source.HasDataToDisplay())
                {
                    var leftRowHeight = verticalCellInset
                                        + ReportsSummaryCollectionViewCell.Height
                                        + verticalCellInset * 2
                                        + ReportsBarChartCollectionViewCell.Height
                                        + verticalCellInset * 2
                                        + ReportAdvancedReportsViaWebCollectionViewCell.Height
                                        + verticalCellInset;
                    var rightRowHeight = verticalCellInset
                                        + ReportsDonutChartCollectionViewCell.Height
                                        + verticalCellInset * 2
                                        + ReportsDonutChartLegendCollectionViewCell.Height * source.NumberOfDonutChartLegendItems()
                                        + verticalCellInset;
                    height = Math.Max(leftRowHeight, rightRowHeight);
                }
                return new CGSize(width, height);
            }
        }

        public override void PrepareLayout()
        {
            layoutAttributes = new List<UICollectionViewLayoutAttributes>();
            var horizontalStartPoint = (CollectionView.Bounds.Width - CollectionViewContentSize.Width) / 2;
            var columnWidth = CollectionViewContentSize.Width / 2 - horizontalCellInset * 2;
            for (var i = 0; i < CollectionView.NumberOfItemsInSection(0); i++)
            {
                var indexPath = NSIndexPath.FromItemSection(i, 0);
                UICollectionViewLayoutAttributes attributes = UICollectionViewLayoutAttributes.CreateForCell(indexPath);
                var cellType = source.CellTypeAt(indexPath);
                switch (cellType)
                {
                    case ReportsCollectionViewCell.Summary:
                        attributes.Frame = new CGRect(
                            horizontalStartPoint + horizontalCellInset,
                            verticalCellInset,
                            columnWidth,
                            ReportsSummaryCollectionViewCell.Height);
                        break;
                    case ReportsCollectionViewCell.BarChartPlaceholder:
                    case ReportsCollectionViewCell.BarChart:
                        attributes.Frame = new CGRect(
                            horizontalStartPoint + horizontalCellInset,
                            verticalCellInset * 3 + ReportsSummaryCollectionViewCell.Height,
                            columnWidth,
                            ReportsBarChartCollectionViewCell.Height);
                        break;
                    case ReportsCollectionViewCell.AdvancedReportsViaWeb:
                        attributes.Frame = new CGRect(
                            horizontalStartPoint + horizontalCellInset,
                            verticalCellInset * 3
                                + ReportsSummaryCollectionViewCell.Height
                                + verticalCellInset * 2
                                + ReportsBarChartCollectionViewCell.Height,
                            columnWidth,
                            ReportAdvancedReportsViaWebCollectionViewCell.Height);
                        break;
                    case ReportsCollectionViewCell.DonutChart:
                        attributes.Frame = new CGRect(
                            horizontalStartPoint + CollectionViewContentSize.Width / 2 + horizontalCellInset,
                            verticalCellInset,
                            columnWidth,
                            ReportsDonutChartCollectionViewCell.Height);
                        break;
                    case ReportsCollectionViewCell.DonutChartLegendItem:
                        attributes.Frame = new CGRect(
                            horizontalStartPoint + CollectionViewContentSize.Width / 2 + horizontalCellInset,
                            verticalCellInset
                                + ReportsDonutChartCollectionViewCell.Height
                                + ReportsDonutChartLegendCollectionViewCell.Height * (indexPath.Item - (CollectionView.NumberOfItemsInSection(0) - source.NumberOfDonutChartLegendItems())),
                            columnWidth,
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
    }
}
