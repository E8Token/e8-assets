using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Auth.Models;
using Energy8.Firebase.Auth.Providers;
using UnityEngine;
using Energy8.Firebase.Auth.Api;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.Firebase.Auth.WebGL.Providers
{
    /// <summary>
    /// WebGL implementation of Firebase Auth provider using JavaScript interop
    /// </summary>
    public class WebFirebaseAuthProvider : BaseFirebaseAuthProvider
    {
        private FirebaseAuthPlugin plugin;

        public override async Task<bool> InitializeAsync()
        {
            try
            {
                plugin = FirebaseAuthPlugin.Instance;
                await plugin.InitializeAsync();
                
                Debug.Log("[WebFirebaseAuthProvider] Initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebFirebaseAuthProvider] Initialization failed: {ex.Message}");
                return false;
            }
        }

        public override async Task<AuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var result = await plugin.SignInWithEmailAndPasswordAsync(email, password);
                var authResult = new AuthResult
                {
                    User = ParseFirebaseUser(result),
                    AdditionalUserInfo = new AdditionalUserInfo { IsNewUser = false }
                };
                
                OnUserSignedIn(authResult.User);
                return authResult;
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task<AuthResult> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var result = await plugin.CreateUserWithEmailAndPasswordAsync(email, password);
                var authResult = new AuthResult
                {
                    User = ParseFirebaseUser(result),
                    AdditionalUserInfo = new AdditionalUserInfo { IsNewUser = true }
                };
                
                OnUserSignedIn(authResult.User);
                return authResult;
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task<AuthResult> SignInAnonymouslyAsync()
        {
            try
            {
                var result = await plugin.SignInAnonymouslyAsync();
                var authResult = new AuthResult
                {
                    User = ParseFirebaseUser(result),
                    AdditionalUserInfo = new AdditionalUserInfo { IsNewUser = true }
                };
                
                OnUserSignedIn(authResult.User);
                return authResult;
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task<AuthResult> SignInWithCredentialAsync(AuthCredential credential)
        {
            try
            {
                var result = await plugin.SignInWithCredentialAsync(credential.ProviderId, credential.Token);
                var authResult = new AuthResult
                {
                    User = ParseFirebaseUser(result),
                    AdditionalUserInfo = new AdditionalUserInfo { IsNewUser = false }
                };
                
                OnUserSignedIn(authResult.User);
                return authResult;
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task SignOutAsync()
        {
            try
            {
                await plugin.SignOutAsync();
                OnUserSignedOut();
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task<FirebaseUser> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await plugin.GetCurrentUserAsync();
                return ParseFirebaseUser(userJson);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WebFirebaseAuthProvider] Failed to get current user: {ex.Message}");
                return null;
            }
        }

        public override async Task DeleteUserAsync()
        {
            try
            {
                await plugin.DeleteUserAsync();
                OnUserSignedOut();
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await plugin.SendPasswordResetEmailAsync(email);
            }
            catch (Exception ex)
            {
                throw new FirebaseAuthException(AuthErrorCode.Unknown, ex.Message, ex);
            }
        }

        public override async Task<bool> ConnectToEmulatorAsync(string host, int port)
        {
            try
            {
                await plugin.ConnectToEmulatorAsync(host, port);
                Debug.Log($"[WebFirebaseAuthProvider] Connected to emulator at {host}:{port}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebFirebaseAuthProvider] Failed to connect to emulator: {ex.Message}");
                return false;
            }
        }

        private FirebaseUser ParseFirebaseUser(string userJson)
        {
            if (string.IsNullOrEmpty(userJson) || userJson == "null")
                return null;

            try
            {
                var userData = JsonUtility.FromJson<WebFirebaseUserData>(userJson);
                return new FirebaseUser
                {
                    Uid = userData.uid,
                    Email = userData.email,
                    DisplayName = userData.displayName,
                    PhotoUrl = userData.photoURL,
                    IsEmailVerified = userData.emailVerified,
                    IsAnonymous = userData.isAnonymous,
                    Metadata = new UserMetadata
                    {
                        CreationTimestamp = DateTime.UtcNow, // WebGL doesn't provide these timestamps
                        LastSignInTimestamp = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebFirebaseAuthProvider] Failed to parse user data: {ex.Message}");
                return null;
            }
        }

        [Serializable]
        private class WebFirebaseUserData
        {
            public string uid;
            public string email;
            public string displayName;
            public string photoURL;
            public bool emailVerified;
            public bool isAnonymous;
        }
    }
}
