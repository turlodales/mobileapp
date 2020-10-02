namespace Toggl.Core.UI.ViewModels.Reports
{
    public class ReportProjectsBarChartPlaceholderElement : IReportElement
    {
        public bool Equals(IReportElement other)
        {
            if (other is ReportProjectsBarChartPlaceholderElement)
                return true;
            return false;
        }
    }
}
