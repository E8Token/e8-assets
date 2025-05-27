using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Providers
{
    public class CustomTokenAuthCredential : AuthCredential
    {
        public string Token { get; }
        
        internal CustomTokenAuthCredential(string token) 
            : base(CustomTokenAuthProvider.ProviderId, CustomTokenAuthProvider.ProviderId)
        {
            Token = token;
        }
    }
}
