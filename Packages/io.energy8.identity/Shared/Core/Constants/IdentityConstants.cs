namespace Energy8.Identity.Shared.Core.Constants
{
    /// <summary>
    /// API endpoint constants used across the Identity system
    /// </summary>
    public static class ApiEndpoints
    {
        // Authentication endpoints
        public const string AUTH_LOGIN = "auth/login";
        public const string AUTH_LOGOUT = "auth/logout";
        public const string AUTH_REFRESH = "auth/refresh";
        public const string AUTH_REGISTER = "auth/register";
        public const string AUTH_VERIFY = "auth/verify";
        
        // User management endpoints
        public const string USER_PROFILE = "user";
        public const string USER_UPDATE = "user/update";
        public const string USER_DELETE = "user/delete";
        public const string USER_CHANGE_EMAIL = "user/email/change";
        public const string USER_CONFIRM_EMAIL = "user/email/confirm";
        
        // Game endpoints
        public const string GAME_SESSION = "game/session";
        public const string GAME_BALANCE = "game/balance";
        public const string GAME_SERVERS = "game/servers";
        public const string GAME_REFERRALS = "game/referrals";
        
        // Analytics endpoints
        public const string ANALYTICS_TRACK = "analytics/track";
        public const string ANALYTICS_EVENT = "analytics/event";
        public const string ANALYTICS_BATCH = "analytics/batch";
    }
    
    /// <summary>
    /// HTTP header constants
    /// </summary>
    public static class HttpHeaders
    {
        public const string AUTHORIZATION = "Authorization";
        public const string BEARER_PREFIX = "Bearer ";
        public const string CONTENT_TYPE = "Content-Type";
        public const string APPLICATION_JSON = "application/json";
        public const string USER_AGENT = "User-Agent";
        public const string X_REQUEST_ID = "X-Request-ID";
        public const string X_CLIENT_VERSION = "X-Client-Version";
    }
    
    /// <summary>
    /// Error code constants
    /// </summary>
    public static class ErrorCodes
    {
        // Authentication errors
        public const string AUTH_INVALID_CREDENTIALS = "AUTH_001";
        public const string AUTH_TOKEN_EXPIRED = "AUTH_002";
        public const string AUTH_TOKEN_INVALID = "AUTH_003";
        public const string AUTH_USER_NOT_FOUND = "AUTH_004";
        public const string AUTH_ALREADY_EXISTS = "AUTH_005";
        
        // Validation errors
        public const string VALIDATION_REQUIRED_FIELD = "VAL_001";
        public const string VALIDATION_INVALID_FORMAT = "VAL_002";
        public const string VALIDATION_TOO_SHORT = "VAL_003";
        public const string VALIDATION_TOO_LONG = "VAL_004";
        
        // Network errors
        public const string NETWORK_CONNECTION_FAILED = "NET_001";
        public const string NETWORK_TIMEOUT = "NET_002";
        public const string NETWORK_UNAVAILABLE = "NET_003";
        
        // Server errors
        public const string SERVER_INTERNAL_ERROR = "SRV_001";
        public const string SERVER_MAINTENANCE = "SRV_002";
        public const string SERVER_OVERLOADED = "SRV_003";
    }
    
    /// <summary>
    /// PlayerPrefs keys for storing user preferences
    /// </summary>
    public static class PlayerPrefsKeys
    {
        public const string ANALYTICS_PERMISSION = "Analytics_Permission";
        public const string ANALYTICS_PERMISSION_REQUESTED = "Analytics_Permission_Requested";
    }

    /// <summary>
    /// Analytics event names
    /// </summary>
    public static class AnalyticsEvents
    {
        // User events
        public const string USER_LOGIN = "user_login";
        public const string USER_LOGOUT = "user_logout";
        public const string USER_REGISTER = "user_register";
        public const string USER_PROFILE_UPDATE = "user_profile_update";
        
        // Game events
        public const string GAME_SESSION_START = "game_session_start";
        public const string GAME_SESSION_END = "game_session_end";
        public const string GAME_LEVEL_COMPLETE = "game_level_complete";
        public const string GAME_ACHIEVEMENT_UNLOCK = "game_achievement_unlock";
        
        // Error events
        public const string ERROR_OCCURRED = "error_occurred";
        public const string API_ERROR = "api_error";
        public const string NETWORK_ERROR = "network_error";
    }
}
