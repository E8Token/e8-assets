namespace Energy8.Firebase.Auth.Providers
{
    public static class AnonymousAuthProvider
    {
        public const string ProviderId = "anonymous";
        
        public static AnonymousAuthCredential GetCredential()
        {
            return new AnonymousAuthCredential();
        }
    }
}
