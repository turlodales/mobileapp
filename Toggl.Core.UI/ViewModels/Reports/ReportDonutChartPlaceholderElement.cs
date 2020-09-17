using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Toggl.Core.Reports;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.UI.ViewModels.Reports
{
    public sealed class ReportDonutChartPlaceholderElement : CompositeReportElement
    {
        private readonly ChartSegment noProjectSegment;
        private readonly DurationFormat durationFormat;

        public ReportDonutChartPlaceholderElement(ChartSegment noProjectSegment, DurationFormat durationFormat)
        {
            this.noProjectSegment = noProjectSegment;
            this.durationFormat = durationFormat;

            var subElements = new List<IReportElement>
            {
                new ReportDonutChartPlaceholderIllustrationElement(),
            };

            if (!noProjectSegment.Equals(default(ChartSegment)))
                subElements.Add(new ReportProjectsDonutChartLegendItemElement(
                    noProjectSegment.ProjectName,
                    noProjectSegment.Color,
                    String.Empty,
                    noProjectSegment.TrackedTime.ToFormattedString(durationFormat),
                    100));

            SubElements = subElements.ToImmutableList();
        }

        public override bool Equals(IReportElement other)
        {
            if (other is ReportDonutChartPlaceholderElement donutPlaceholderElement)
            {
                return donutPlaceholderElement.noProjectSegment.Equals(noProjectSegment)
                       && donutPlaceholderElement.durationFormat == durationFormat;
            }
            return false;
        }
    }
}
