using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
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

