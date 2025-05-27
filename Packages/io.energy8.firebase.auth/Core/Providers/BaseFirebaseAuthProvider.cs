using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Auth.Api;
using Energy8.Firebase.Auth.Models;

namespace Energy8.Firebase.Auth.Providers
{
    public abstract class BaseFirebaseAuthProvider : IFirebaseAuthApi
    {
        protected FirebaseUser currentUser;
        
        public virtual FirebaseUser CurrentUser => currentUser;
        
        public event Action<FirebaseUser> OnAuthStateChanged;
        public event Action<FirebaseUser> OnIdTokenChanged;
        public event Action<FirebaseAuthException> OnAuthError;
        
        public abstract Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);
        public abstract Task<AuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password, CancellationToken ct = default);
        public abstract Task<AuthResult> SignInWithCredentialAsync(AuthCredential credential, CancellationToken ct = default);
        public abstract Task<AuthResult> SignInAnonymouslyAsync(CancellationToken ct = default);
        public abstract Task SignOutAsync(CancellationToken ct = default);
        public abstract Task SendPasswordResetEmailAsync(string email, CancellationToken ct = default);
        public abstract Task SendEmailVerificationAsync(CancellationToken ct = default);
        public abstract Task UpdateUserProfileAsync(string displayName = null, string photoUrl = null, CancellationToken ct = default);
        public abstract Task UpdatePasswordAsync(string newPassword, CancellationToken ct = default);
        public abstract Task UpdateEmailAsync(string newEmail, CancellationToken ct = default);
        public abstract Task DeleteUserAsync(CancellationToken ct = default);
        public abstract Task ReloadUserAsync(CancellationToken ct = default);
        public abstract Task<string> GetIdTokenAsync(bool forceRefresh = false, CancellationToken ct = default);
        public abstract Task<AuthResult> LinkWithCredentialAsync(AuthCredential credential, CancellationToken ct = default);
        public abstract Task<FirebaseUser> UnlinkAsync(string providerId, CancellationToken ct = default);
        public abstract Task<AuthResult> ReauthenticateAsync(AuthCredential credential, CancellationToken ct = default);
        
        protected void InvokeAuthStateChanged(FirebaseUser user)
        {
            currentUser = user;
            OnAuthStateChanged?.Invoke(user);
        }
        
        protected void InvokeIdTokenChanged(FirebaseUser user)
        {
            OnIdTokenChanged?.Invoke(user);
        }
        
        protected void InvokeAuthError(FirebaseAuthException error)
        {
            OnAuthError?.Invoke(error);
        }

        /// <summary>
        /// Connect to Firebase Auth emulator
        /// </summary>
        public virtual async Task<bool> ConnectToEmulatorAsync(string host, int port)
        {
            await Task.CompletedTask;
            return false;
        }
    }
}
