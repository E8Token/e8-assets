using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class CodeViewResult : ViewResult
    {
        public string Code { get; }

        public CodeViewResult(string code)
        {
            Code = code;
        }
    }
}

