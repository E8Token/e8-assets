using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Energy8.Models.Errors;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Plugins.WebGL.Firebase;
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

        static public bool IsSignedIn
#if UNITY_WEBGL && !UNITY_EDITOR
        => CurrentUser != null;
#else
        => auth.CurrentUser != null && auth.CurrentUser.IsValid();
#endif

        static public event Action<FirebaseUser> OnSignedIn;
        static public event Action OnSignedOut;

#if UNITY_WEBGL && !UNITY_EDITOR
        public static void Initialize(string config)
        {
            FirebaseAuthWebGL.Instance.Initialize(config);
            FirebaseAuthWebGL.Instance.OnSignInEvent += (user) =>
            {
                CurrentUser = user;
                AuthStateChanged();
            };
            FirebaseAuthWebGL.Instance.OnSignOutEvent += () => 
            {
                CurrentUser = null;
                AuthStateChanged();
            };
        }
#else
        public static void Initialize(FirebaseApp app)
        {
            auth = FirebaseAuth.GetAuth(app);
            auth.StateChanged += AuthStateChanged;
            //AuthStateChanged(null, null);
            logger.Log("Initialized");
        }
#endif
        public static async UniTask<string> GetAuthTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return await FirebaseAuthWebGL.Instance.GetIdTokenAsync(cancellationToken, forceRefresh);
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
            return await FirebaseAuthWebGL.Instance.SignInByTokenAsync(cancellationToken, token);
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
        static void AuthStateChanged()
        {
            if (CurrentUser != _user)
            {
                bool signedIn = _user != CurrentUser && CurrentUser != null;
                if (!signedIn && _user != null)
                {
                    logger.Log("Signed out " + _user.UserId);
                    OnSignedOut?.Invoke();
                }
                _user = CurrentUser;
                if (signedIn)
                {
                    logger.Log("Signed in " + _user.UserId);
                    OnSignedIn?.Invoke(_user);
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
                    OnSignedOut?.Invoke();
                }
                _user = auth.CurrentUser;
                if (signedIn)
                {
                    logger.Log("Signed in " + _user.UserId);
                    OnSignedIn?.Invoke(_user);
                }
            }
        }
        public static void SignOut() => auth.SignOut();
#endif
    }
}