#if UNITY_WEBGL //&& !UNITY_EDITOR
using System;

namespace Energy8.Models.WebGL.Firebase
{
    public class FirebaseUser
    {
        public string DisplayName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public bool IsAnonymous { get; private set; }

        public bool IsEmailVerified { get; private set; }

        public UserMetadata Metadata { get; private set; }

        public string PhoneNumber { get; private set; } = string.Empty;

        public Uri PhotoUrl { get; private set; }

        //public IEnumerable<IUserInfo> ProviderData { get; private set; }

        public string ProviderId { get; private set; } = string.Empty;

        public string UserId { get; private set; } = string.Empty;
    }
    public class UserMetadata
    {
        public ulong LastSignInTimestamp
        {
            get;
            private set;
        }

        public ulong CreationTimestamp
        {
            get;
            private set;
        }
    }
}
#endif