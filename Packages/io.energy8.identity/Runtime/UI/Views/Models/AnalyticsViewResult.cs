using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models 
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