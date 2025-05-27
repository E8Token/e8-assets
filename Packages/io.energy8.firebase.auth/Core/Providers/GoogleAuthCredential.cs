using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Providers
{
    public class GoogleAuthCredential : AuthCredential
    {
        public string IdToken { get; }
        public string AccessToken { get; }
        
        internal GoogleAuthCredential(string idToken, string accessToken) 
            : base(GoogleAuthProvider.ProviderId, GoogleAuthProvider.ProviderId)
        {
            IdToken = idToken;
            AccessToken = accessToken;
        }
    }
}
