using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
{
    public class ChangeNameViewResult : ViewResult
    {
        public string Name { get; }

        public ChangeNameViewResult(string name)
        {
            Name = name;
        }
    }
}
