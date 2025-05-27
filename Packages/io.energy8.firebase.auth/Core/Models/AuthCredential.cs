namespace Energy8.Firebase.Auth.Models
{
    public class AuthCredential
    {
        public string ProviderId { get; set; }
        public string SignInMethod { get; set; }
        
        protected AuthCredential(string providerId, string signInMethod)
        {
            ProviderId = providerId;
            SignInMethod = signInMethod;
        }
    }
}
