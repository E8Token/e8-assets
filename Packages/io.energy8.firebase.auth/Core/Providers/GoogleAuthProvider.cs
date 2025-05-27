namespace Energy8.Firebase.Auth.Providers
{
    public static class GoogleAuthProvider
    {
        public const string ProviderId = "google.com";
        
        public static GoogleAuthCredential GetCredential(string idToken, string accessToken = null)
        {
            return new GoogleAuthCredential(idToken, accessToken);
        }
    }
}
