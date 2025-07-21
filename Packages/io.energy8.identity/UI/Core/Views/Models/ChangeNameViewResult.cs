using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
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

