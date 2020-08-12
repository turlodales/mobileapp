using Toggl.Shared.Models;

namespace Toggl.Core.UI.ViewModels.Reports
{
    public sealed class ReportAdvancedReportsViaWebElement : IReportElement
    {
        public bool ShouldShowAvailableOnOtherPlans { get; }

        public ReportAdvancedReportsViaWebElement(Plan currentPlan)
        {
            ShouldShowAvailableOnOtherPlans = currentPlan == Plan.Free;
        }

        public bool Equals(IReportElement other)
            =>  other is ReportAdvancedReportsViaWebElement;
    }
}
