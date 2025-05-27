using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Auth.Api;
using Energy8.Firebase.Auth.Models;
using Energy8.Firebase.Auth.Providers;
using Energy8.Firebase.Auth.Configuration;
using Energy8.Firebase.Core;
using Energy8.Firebase.Core.Configuration;
using Energy8.Firebase.Core.Configuration.Models;
using UnityEngine;

namespace Energy8.Firebase.Auth
{
    /// <summary>
    /// Entry point for Firebase Authentication
    /// </summary>
    public static class FirebaseAuth
    {
        private static IFirebaseAuthApi authApi;
        private static bool isInitialized = false;
        
        public static FirebaseUser CurrentUser => authApi?.CurrentUser;
        
        public static event Action<FirebaseUser> OnAuthStateChanged;
        public static event Action<FirebaseUser> OnIdTokenChanged;
        public static event Action<FirebaseAuthException> OnAuthError;
        
        static FirebaseAuth()
        {
            InitializeProvider();
        }
        
        private static void InitializeProvider()
        {
            // Provider будет инициализирован через конкретные платформенные assembly
            // Native assembly инициализирует NativeFirebaseAuthProvider
            // WebGL assembly инициализирует WebFirebaseAuthProvider
            
            if (authApi == null)
            {
                Debug.LogWarning("[FirebaseAuth] No platform provider available. Make sure Native or WebGL assemblies are included.");
                return;
            }
            
            authApi.OnAuthStateChanged += (user) => OnAuthStateChanged?.Invoke(user);
            authApi.OnIdTokenChanged += (user) => OnIdTokenChanged?.Invoke(user);
            authApi.OnAuthError += (error) => OnAuthError?.Invoke(error);
            
            isInitialized = true;
            Debug.Log("[FirebaseAuth] Provider initialized successfully");
        }
        
        public static void SetProvider(IFirebaseAuthApi provider)
        {
            authApi = provider;
            
            if (!isInitialized)
            {
                InitializeProvider();
            }
        }
        
        /// <summary>
        /// Sign in with email and password
        /// </summary>
        public static async Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.SignInWithEmailAndPasswordAsync(email, password, ct);
        }
        
        /// <summary>
        /// Create user with email and password
        /// </summary>
        public static async Task<AuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.CreateUserWithEmailAndPasswordAsync(email, password, ct);
        }
        
        /// <summary>
        /// Sign in with credential
        /// </summary>
        public static async Task<AuthResult> SignInWithCredentialAsync(AuthCredential credential, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.SignInWithCredentialAsync(credential, ct);
        }
        
        /// <summary>
        /// Sign in anonymously
        /// </summary>
        public static async Task<AuthResult> SignInAnonymouslyAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.SignInAnonymouslyAsync(ct);
        }
        
        /// <summary>
        /// Sign out current user
        /// </summary>
        public static async Task SignOutAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.SignOutAsync(ct);
        }
        
        /// <summary>
        /// Send password reset email
        /// </summary>
        public static async Task SendPasswordResetEmailAsync(string email, CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.SendPasswordResetEmailAsync(email, ct);
        }
        
        /// <summary>
        /// Send email verification
        /// </summary>
        public static async Task SendEmailVerificationAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.SendEmailVerificationAsync(ct);
        }
        
        /// <summary>
        /// Update user profile
        /// </summary>
        public static async Task UpdateUserProfileAsync(string displayName = null, string photoUrl = null, CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.UpdateUserProfileAsync(displayName, photoUrl, ct);
        }
        
        /// <summary>
        /// Update user password
        /// </summary>
        public static async Task UpdatePasswordAsync(string newPassword, CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.UpdatePasswordAsync(newPassword, ct);
        }
        
        /// <summary>
        /// Update user email
        /// </summary>
        public static async Task UpdateEmailAsync(string newEmail, CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.UpdateEmailAsync(newEmail, ct);
        }
        
        /// <summary>
        /// Delete current user
        /// </summary>
        public static async Task DeleteUserAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.DeleteUserAsync(ct);
        }
        
        /// <summary>
        /// Reload current user
        /// </summary>
        public static async Task ReloadUserAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await authApi.ReloadUserAsync(ct);
        }
        
        /// <summary>
        /// Get ID token for current user
        /// </summary>
        public static async Task<string> GetIdTokenAsync(bool forceRefresh = false, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.GetIdTokenAsync(forceRefresh, ct);
        }
        
        /// <summary>
        /// Link credential to current user
        /// </summary>
        public static async Task<AuthResult> LinkWithCredentialAsync(AuthCredential credential, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.LinkWithCredentialAsync(credential, ct);
        }
        
        /// <summary>
        /// Unlink provider from current user
        /// </summary>
        public static async Task<FirebaseUser> UnlinkAsync(string providerId, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.UnlinkAsync(providerId, ct);
        }
        
        /// <summary>
        /// Reauthenticate current user
        /// </summary>
        public static async Task<AuthResult> ReauthenticateAsync(AuthCredential credential, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await authApi.ReauthenticateAsync(credential, ct);
        }        /// <summary>
        /// Initialize Firebase Auth with configuration
        /// </summary>
        public static async Task InitializeAsync(CancellationToken ct = default)
        {
            await InitializeAsync();
        }
        
        /// <summary>
        /// Initialize Firebase Auth with configuration
        /// </summary>
        public static async Task InitializeAsync()
        {
            // Initialize Firebase Core first
            var config = FirebaseCoreConfiguration.GetConfigForCurrentEnvironment(FirebaseCore.GetCurrentPlatform());
            if (!string.IsNullOrEmpty(config))
            {
                await FirebaseCore.InitializeAppAsync(config);
            }
            
            // Configure emulator if enabled
            if (FirebaseAuthConfiguration.UseEmulator && authApi != null)
            {
                await authApi.ConnectToEmulatorAsync(FirebaseAuthConfiguration.EmulatorHost, FirebaseAuthConfiguration.EmulatorPort);
            }
            
            // Auto-sign in if enabled and user exists
            if (FirebaseAuthConfiguration.EnableAutoSignIn && authApi != null)
            {
                try
                {
                    var user = authApi.CurrentUser;
                    if (user != null)
                    {
                        OnAuthStateChanged?.Invoke(user);
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"Auto sign-in failed: {ex.Message}");
                }
            }
        }
        
        private static void EnsureInitialized()
        {
            if (authApi == null)
            {
                throw new InvalidOperationException("[FirebaseAuth] Provider not initialized. Make sure Firebase Auth platform assemblies are included.");
            }
        }
          /// <summary>
        /// Sign in with custom token
        /// </summary>
        public static async Task<AuthResult> SignInWithCustomTokenAsync(string customToken, CancellationToken ct = default)
        {
            EnsureInitialized();
            var credential = CustomTokenAuthProvider.GetCredential(customToken);
            return await authApi.SignInWithCredentialAsync(credential, ct);
        }
    }
}
