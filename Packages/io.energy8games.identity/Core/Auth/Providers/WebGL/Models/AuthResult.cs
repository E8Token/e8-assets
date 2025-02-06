#if UNITY_WEBGL && !UNITY_EDITOR
using System;

namespace Energy8.Identity.Core.Auth.Models
{
    [Serializable]
    public class AuthResult
    {
        public bool Success { get; }
        public FirebaseUser User { get; }
        public string Error { get; }
        public string Token { get; }

        public AuthResult(bool success, FirebaseUser user = null, string error = null, string token = null)
        {
            Success = success;
            User = user;
            Error = error;
            Token = token;
        }
    }
}
#endif