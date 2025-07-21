using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class UserViewResult : ViewResult
    {
        public UserAction Action { get; }

        public UserViewResult(UserAction action)
        {
            Action = action;
        }
    }

    public enum UserAction
    {
        OpenSettings,
        SignOut
    }
}

