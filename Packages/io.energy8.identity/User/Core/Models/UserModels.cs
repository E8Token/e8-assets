using System;

namespace Energy8.Identity.User.Core.Models
{
    /// <summary>
    /// Represents user service operation result
    /// </summary>
    [Serializable]
    public class UserOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        
        public UserOperationResult(bool success, string message = null, string errorCode = null)
        {
            Success = success;
            Message = message;
            ErrorCode = errorCode;
        }
        
        public static UserOperationResult CreateSuccess(string message = "Operation completed successfully")
        {
            return new UserOperationResult(true, message);
        }
        
        public static UserOperationResult CreateError(string message, string errorCode = null)
        {
            return new UserOperationResult(false, message, errorCode);
        }
    }
    
    /// <summary>
    /// User preferences and settings
    /// </summary>
    [Serializable]
    public class UserPreferences
    {
        public bool NotificationsEnabled { get; set; } = true;
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "default";
        public bool AnalyticsOptIn { get; set; } = true;
        
        public UserPreferences()
        {
        }
    }
    
    /// <summary>
    /// User validation request model
    /// </summary>
    [Serializable]
    public class UserValidationRequest
    {
        public string Token { get; set; }
        public string Code { get; set; }
        public string RequestType { get; set; } // "email_change", "delete_account", etc.
        
        public UserValidationRequest(string token, string code, string requestType)
        {
            Token = token;
            Code = code;
            RequestType = requestType;
        }
    }
}
