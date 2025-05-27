namespace Energy8.Firebase.Auth.Models
{
    public class FirebaseUser
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string PhotoUrl { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsAnonymous { get; set; }
        public string ProviderId { get; set; }
        public UserMetadata Metadata { get; set; }
    }
}
