using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Energy8.WebGL.PluginPlatform;
using UnityEngine;

namespace Energy8.Firebase.Auth.WebGL.Scripts
{
    /// <summary>
    /// Firebase Auth WebGL plugin for JavaScript interop
    /// </summary>
    public class FirebaseAuthPlugin : WebGLPlugin<FirebaseAuthPlugin>
    {
        // JavaScript function imports
        [DllImport("__Internal")]
        private static extern void FirebaseAuth_Initialize();

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_SignInWithEmailAndPassword(string email, string password, string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_CreateUserWithEmailAndPassword(string email, string password, string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_SignInAnonymously(string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_SignInWithCredential(string providerId, string token, string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_SignOut(string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_GetCurrentUser(string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_DeleteUser(string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_SendPasswordResetEmail(string email, string callbackId);

        [DllImport("__Internal")]
        private static extern void FirebaseAuth_ConnectToEmulator(string host, int port, string callbackId);

        public override async Task InitializeAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            FirebaseAuth_Initialize();
            await Task.CompletedTask;
#else
            await Task.CompletedTask;
            Debug.LogWarning("[FirebaseAuthPlugin] WebGL plugin called outside WebGL platform");
#endif
        }

        public async Task<string> SignInWithEmailAndPasswordAsync(string email, string password)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_SignInWithEmailAndPassword(email, password, callbackId);
            return await tcs.Task;
#else
            await Task.CompletedTask;
            return "{\"uid\":\"test-uid\",\"email\":\"" + email + "\",\"emailVerified\":false,\"isAnonymous\":false}";
#endif
        }

        public async Task<string> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_CreateUserWithEmailAndPassword(email, password, callbackId);
            return await tcs.Task;
#else
            await Task.CompletedTask;
            return "{\"uid\":\"test-uid\",\"email\":\"" + email + "\",\"emailVerified\":false,\"isAnonymous\":false}";
#endif
        }

        public async Task<string> SignInAnonymouslyAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_SignInAnonymously(callbackId);
            return await tcs.Task;
#else
            await Task.CompletedTask;
            return "{\"uid\":\"test-anonymous-uid\",\"email\":null,\"emailVerified\":false,\"isAnonymous\":true}";
#endif
        }

        public async Task<string> SignInWithCredentialAsync(string providerId, string token)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_SignInWithCredential(providerId, token, callbackId);
            return await tcs.Task;
#else
            await Task.CompletedTask;
            return "{\"uid\":\"test-uid\",\"email\":\"test@example.com\",\"emailVerified\":true,\"isAnonymous\":false}";
#endif
        }

        public async Task SignOutAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_SignOut(callbackId);
            await tcs.Task;
#else
            await Task.CompletedTask;
#endif
        }

        public async Task<string> GetCurrentUserAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_GetCurrentUser(callbackId);
            return await tcs.Task;
#else
            await Task.CompletedTask;
            return "null";
#endif
        }

        public async Task DeleteUserAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_DeleteUser(callbackId);
            await tcs.Task;
#else
            await Task.CompletedTask;
#endif
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_SendPasswordResetEmail(email, callbackId);
            await tcs.Task;
#else
            await Task.CompletedTask;
#endif
        }

        public async Task ConnectToEmulatorAsync(string host, int port)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var callbackId = GenerateCallbackId();
            var tcs = CreateCallback<string>(callbackId);
            
            FirebaseAuth_ConnectToEmulator(host, port, callbackId);
            await tcs.Task;
#else
            await Task.CompletedTask;
#endif
        }

        // JavaScript callback handlers
        [JSCallable]
        public void OnSignInSuccess(string callbackId, string userJson)
        {
            CompleteCallback(callbackId, userJson);
        }

        [JSCallable]
        public void OnSignInError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Sign in failed: {error}"));
        }

        [JSCallable]
        public void OnCreateUserSuccess(string callbackId, string userJson)
        {
            CompleteCallback(callbackId, userJson);
        }

        [JSCallable]
        public void OnCreateUserError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Create user failed: {error}"));
        }

        [JSCallable]
        public void OnSignOutSuccess(string callbackId)
        {
            CompleteCallback(callbackId, "success");
        }

        [JSCallable]
        public void OnSignOutError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Sign out failed: {error}"));
        }

        [JSCallable]
        public void OnGetCurrentUserSuccess(string callbackId, string userJson)
        {
            CompleteCallback(callbackId, userJson);
        }

        [JSCallable]
        public void OnGetCurrentUserError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Get current user failed: {error}"));
        }

        [JSCallable]
        public void OnDeleteUserSuccess(string callbackId)
        {
            CompleteCallback(callbackId, "success");
        }

        [JSCallable]
        public void OnDeleteUserError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Delete user failed: {error}"));
        }

        [JSCallable]
        public void OnPasswordResetSuccess(string callbackId)
        {
            CompleteCallback(callbackId, "success");
        }

        [JSCallable]
        public void OnPasswordResetError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Password reset failed: {error}"));
        }

        [JSCallable]
        public void OnEmulatorConnectSuccess(string callbackId)
        {
            CompleteCallback(callbackId, "success");
        }

        [JSCallable]
        public void OnEmulatorConnectError(string callbackId, string error)
        {
            CompleteCallbackWithError(callbackId, new Exception($"Emulator connection failed: {error}"));
        }
    }
}
