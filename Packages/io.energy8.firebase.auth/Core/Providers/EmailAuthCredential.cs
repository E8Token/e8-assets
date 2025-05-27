using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Providers
{
    public class EmailAuthCredential : AuthCredential
    {
        public string Email { get; }
        public string Password { get; }
        
        internal EmailAuthCredential(string email, string password) 
            : base(EmailAuthProvider.ProviderId, EmailAuthProvider.ProviderId)
        {
            Email = email;
            Password = password;
        }
    }
}
