using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Energy8.Models.Errors;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Firebase.WebGL;
using Energy8.Models.WebGL.Firebase;
#else
using Firebase;
using Firebase.Auth;
#endif

namespace Energy8.Firebase
{
    public class AuthController
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        static FirebaseAuth auth;
#endif

        static FirebaseUser _user;

        static readonly Logger logger = new(null, "AuthController", new Color(0.5f, 0.1f, 0.6f));

#if UNITY_WEBGL && !UNITY_EDITOR
        public static FirebaseUser CurrentUser { get; private set; }
#endif

        static public bool IsAutorized
#if UNITY_WEBGL && !UNITY_EDITOR
        => CurrentUser != null;
#else
        => auth.CurrentUser != null && auth.CurrentUser.IsValid();
#endif

        static public event Action<string> OnSignInEvent;
        static public event Action OnSignOutEvent;

#if UNITY_WEBGL && !UNITY_EDITOR
        public static void Initialize(string config)
        {
            FirebaseAuthWebGL.Initialize(config);
            FirebaseAuthWebGL.OnSignInEvent += (user) =>
            {
                CurrentUser = user;
                AuthStateChanged(user);
            };
            FirebaseAuthWebGL.OnSignOutEvent += () => 
            {
                CurrentUser = null;
                AuthStateChanged(null);
            };
        }
#else
        public static void Initialize(FirebaseApp app)
        {
            auth = FirebaseAuth.GetAuth(app);
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(null, null);
            logger.Log("Initialized");
        }
#endif
        public static async UniTask<string> GetAuthTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return await FirebaseAuthWebGL.GetIdTokenAsync(cancellationToken, forceRefresh);
#else
            try
            {
                return await auth.CurrentUser.TokenAsync(forceRefresh).AsUniTask().AttachExternalCancellation(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ErrorDataException("Authorization Error", ex.Message, canProceed: true, mustSignOut: true);
            }
#endif
        }

        public static async UniTask<FirebaseUser> SignInByTokenAsync(CancellationToken cancellationToken, string token)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return await FirebaseAuthWebGL.SignInByTokenAsync(cancellationToken, token);
#else
            try
            {
                return (await auth.SignInWithCustomTokenAsync(token).AsUniTask().AttachExternalCancellation(cancellationToken)).User;
            }
            catch (Exception ex)
            {
                throw new ErrorDataException("Authorization Error", ex.Message, canProceed: true, mustSignOut: true);
            }
#endif
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        static void AuthStateChanged(FirebaseUser user)
        {
            if (CurrentUser != _user)
            {
                bool signedIn = _user != CurrentUser && CurrentUser != null;
                if (!signedIn && _user != null)
                {
                    logger.Log("Signed out " + user.UserId);
                    OnSignOutEvent?.Invoke();
                }
                user = CurrentUser;
                if (signedIn)
                {
                    logger.Log("Signed in " + user.UserId);
                    OnSignInEvent?.Invoke(user.UserId);
                }
            }
        }
        public static void SignOut() => FirebaseAuthWebGL.SignOut();
#else
        public static async UniTask<AuthResult> SignInByGoogleAsync(string token, CancellationToken cancellationToken)
        {
            Credential credential = GoogleAuthProvider.GetCredential(token, null);
            Task<AuthResult> task = auth.SignInAndRetrieveDataWithCredentialAsync(credential);
            await task.AsUniTask().AttachExternalCancellation(cancellationToken);

            if (task.IsCanceled)
                throw new Exception("SignInAndRetrieveDataWithCredentialAsync was canceled.");
            if (task.IsFaulted)
                throw new Exception("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);

            return task.Result;
        }

        static void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (auth.CurrentUser != _user)
            {
                bool signedIn = _user != auth.CurrentUser && auth.CurrentUser != null
                    && auth.CurrentUser.IsValid();
                if (!signedIn && _user != null)
                {
                    logger.Log("Signed out " + _user.UserId);
                    OnSignOutEvent?.Invoke();
                }
                _user = auth.CurrentUser;
                if (signedIn)
                {
                    logger.Log("Signed in " + _user.UserId);
                    OnSignInEvent?.Invoke(_user.UserId);
                }
            }
        }
        public static void SignOut() => auth.SignOut();
#endif
    }
}