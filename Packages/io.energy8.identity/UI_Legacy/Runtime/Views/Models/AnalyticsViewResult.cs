using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models 
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
