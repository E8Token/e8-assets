namespace Energy8.Firebase.Auth.Providers
{
    public static class EmailAuthProvider
    {
        public const string ProviderId = "password";
        
        public static EmailAuthCredential GetCredential(string email, string password)
        {
            return new EmailAuthCredential(email, password);
        }
    }
}
