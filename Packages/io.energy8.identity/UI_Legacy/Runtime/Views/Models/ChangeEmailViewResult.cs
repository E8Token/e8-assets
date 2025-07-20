using Energy8.Identity.UI.Runtime.Views.Base;

namespace Energy8.Identity.UI.Runtime.Views.Models
{
    public class ChangeEmailViewResult : ViewResult
    {
        public string Email { get; }

        public ChangeEmailViewResult(string email)
        {
            Email = email;
        }
    }
}

