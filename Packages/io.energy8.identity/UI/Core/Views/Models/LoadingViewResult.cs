
namespace Energy8.Identity.UI.Core.Views.Models
{
    public class LoadingViewResult : ViewResult
    {
        public object Result { get; }

        public LoadingViewResult()
        {
            Result = default;
        }

        public LoadingViewResult(object result)
        {
            Result = result;
        }
    }
}

