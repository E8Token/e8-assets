namespace Energy8.Firebase.Auth.Models
{
    public class AuthResult
    {
        public FirebaseUser User { get; set; }
        public AuthCredential Credential { get; set; }
        public AdditionalUserInfo AdditionalUserInfo { get; set; }
    }
}
