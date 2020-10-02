namespace Toggl.Core.UI.ViewModels.Reports
{
    public sealed class ReportDonutChartPlaceholderIllustrationElement : IReportElement
    {
        public bool Equals(IReportElement other)
        {
            if (other is ReportDonutChartPlaceholderIllustrationElement)
                return true;
            return false;
        }
    }
}
