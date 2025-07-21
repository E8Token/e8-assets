using Energy8.Identity.UI.Core.Views;

namespace Energy8.Identity.UI.Core.Views.Models
{
    public class SignInViewResult : ViewResult
    {
        public SignInMethod Method { get; }
        public string Email { get; }

        public SignInViewResult(SignInMethod method, string email)
        {
            Method = method;
            Email = email;
        }
    }

    public enum SignInMethod
    {
        Email,
        Google,
        Apple,
        Telegram
    }
}

