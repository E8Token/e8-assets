using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
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
