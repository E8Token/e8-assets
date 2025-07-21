
using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class UserViewParams : ViewParams
    {
        public string Title { get; }

        public UserViewParams(string title)
        {
            Title = title;
        }
    }
}
