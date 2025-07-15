using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models
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

