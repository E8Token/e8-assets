using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
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
