using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Providers
{
    public class AnonymousAuthCredential : AuthCredential
    {
        internal AnonymousAuthCredential() 
            : base(AnonymousAuthProvider.ProviderId, AnonymousAuthProvider.ProviderId)
        {
        }
    }
}
