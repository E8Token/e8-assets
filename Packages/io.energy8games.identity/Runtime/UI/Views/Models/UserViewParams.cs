
using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
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