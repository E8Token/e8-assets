#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using Newtonsoft.Json;

namespace Energy8.Identity.Auth.Core.Models
{
    [Serializable]
    public class FirebaseUser
    {
        [JsonProperty("uid")]
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhotoUrl { get; set; }
        public string ProviderId { get; set; }
        public bool IsAnonymous { get; set; }
        [JsonProperty("emailVerified")]
        public bool IsEmailVerified { get; set; }
    }
}

#endif