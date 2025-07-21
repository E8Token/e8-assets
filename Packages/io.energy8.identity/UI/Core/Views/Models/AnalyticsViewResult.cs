using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models 
{
    public class AnalyticsViewResult : ViewResult
    {
        public bool IsDetailedAnalyticsAllowed { get; }

        public AnalyticsViewResult(bool isAllowed)
        {
            IsDetailedAnalyticsAllowed = isAllowed;
        }
    }
}
