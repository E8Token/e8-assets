namespace Energy8.Firebase.Auth.Providers
{
    public static class CustomTokenAuthProvider
    {
        public const string ProviderId = "custom";
        
        public static CustomTokenAuthCredential GetCredential(string token)
        {
            return new CustomTokenAuthCredential(token);
        }
    }
}
