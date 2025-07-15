
using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models
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
