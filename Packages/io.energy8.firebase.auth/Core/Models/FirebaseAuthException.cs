using System;

namespace Energy8.Firebase.Auth.Models
{
    public class FirebaseAuthException : Exception
    {
        public AuthErrorCode ErrorCode { get; }
        
        public FirebaseAuthException(AuthErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        
        public FirebaseAuthException(AuthErrorCode errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
