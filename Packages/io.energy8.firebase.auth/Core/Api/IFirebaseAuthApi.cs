using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Api
{
    public interface IFirebaseAuthApi
    {
        /// <summary>
        /// Current authenticated user
        /// </summary>
        FirebaseUser CurrentUser { get; }
        
        /// <summary>
        /// Sign in with email and password
        /// </summary>
        Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);
        
        /// <summary>
        /// Create user with email and password
        /// </summary>
        Task<AuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);
        
        /// <summary>
        /// Sign in with credential
        /// </summary>
        Task<AuthResult> SignInWithCredentialAsync(AuthCredential credential, CancellationToken ct = default);
        
        /// <summary>
        /// Sign in anonymously
        /// </summary>
        Task<AuthResult> SignInAnonymouslyAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Sign out current user
        /// </summary>
        Task SignOutAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Send password reset email
        /// </summary>
        Task SendPasswordResetEmailAsync(string email, CancellationToken ct = default);
        
        /// <summary>
        /// Send email verification
        /// </summary>
        Task SendEmailVerificationAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Update user profile
        /// </summary>
        Task UpdateUserProfileAsync(string displayName = null, string photoUrl = null, CancellationToken ct = default);
        
        /// <summary>
        /// Update user password
        /// </summary>
        Task UpdatePasswordAsync(string newPassword, CancellationToken ct = default);
        
        /// <summary>
        /// Update user email
        /// </summary>
        Task UpdateEmailAsync(string newEmail, CancellationToken ct = default);
        
        /// <summary>
        /// Delete current user
        /// </summary>
        Task DeleteUserAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Reload current user
        /// </summary>
        Task ReloadUserAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Get ID token for current user
        /// </summary>
        Task<string> GetIdTokenAsync(bool forceRefresh = false, CancellationToken ct = default);
        
        /// <summary>
        /// Link credential to current user
        /// </summary>
        Task<AuthResult> LinkWithCredentialAsync(AuthCredential credential, CancellationToken ct = default);
        
        /// <summary>
        /// Unlink provider from current user
        /// </summary>
        Task<FirebaseUser> UnlinkAsync(string providerId, CancellationToken ct = default);
        
        /// <summary>
        /// Reauthenticate current user
        /// </summary>
        Task<AuthResult> ReauthenticateAsync(AuthCredential credential, CancellationToken ct = default);
        
        /// <summary>
        /// Connect to Firebase Auth emulator
        /// </summary>
        Task<bool> ConnectToEmulatorAsync(string host, int port);
        
        /// <summary>
        /// Events
        /// </summary>
        event Action<FirebaseUser> OnAuthStateChanged;
        event Action<FirebaseUser> OnIdTokenChanged;
        event Action<FirebaseAuthException> OnAuthError;
    }
}
