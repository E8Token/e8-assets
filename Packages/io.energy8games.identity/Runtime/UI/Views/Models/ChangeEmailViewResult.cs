using Energy8.Identity.Views.Base;

namespace Energy8.Identity.Views.Models
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
