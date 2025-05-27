using System.Collections.Generic;

namespace Energy8.Firebase.Auth.Models
{
    public class AdditionalUserInfo
    {
        public bool IsNewUser { get; set; }
        public string ProviderId { get; set; }
        public Dictionary<string, object> Profile { get; set; }
        public string Username { get; set; }
    }
}
