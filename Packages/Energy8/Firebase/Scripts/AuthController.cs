using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Energy8.Models.Errors;


#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Firebase.WebGL;
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
        static FirebaseUser user;
#endif

        static readonly Logger logger = new(null, "AuthController", new Color(0.5f, 0.1f, 0.6f));

        static public bool IsAutorized
#if UNITY_WEBGL && !UNITY_EDITOR
        { get; private set; } = false;
#else
        => auth.CurrentUser != null && auth.CurrentUser.IsValid();
#endif

        static public event Action<string> OnSignIn;
        static public event Action OnSignOut;

        public static void Initialize(FirebaseApp app)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            FirebaseAuthWebGL.Initialize();
            FirebaseAuthWebGL.OnSignIn += (uid) => AuthStateChanged(true, uid);
            FirebaseAuthWebGL.OnSignOut += () => AuthStateChanged(false, string.Empty);
#else
            auth = FirebaseAuth.GetAuth(app);
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(null, null);
            logger.Log("Initialized");
#endif
        }
        public static async UniTask<string> GetAuthTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            UniTask<string> task = FirebaseAuthWebGL.GetIdTokenAsync(cancellationToken, forceRefresh);
            return TryResult<string>.CreateSuccessful(await task);
#else
            Task<string> task = auth.CurrentUser.TokenAsync(forceRefresh);
            try
            {
                await task.AsUniTask().AttachExternalCancellation(cancellationToken);
                throw new ErrorDataException("Authorization Error", task.Exception.Message, canProceed: true, mustSignOut: true);
            }
            catch
            {
                return task.Result;
            }
#endif
        }

        public static async UniTask<FirebaseUser> SignInByTokenAsync(CancellationToken cancellationToken, string token)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            UniTask<string> task = FirebaseAuthWebGL.SignInByTokenAsync(cancellationToken, token);
            return TryResult<string>.CreateSuccessful(await task);
#else
            Task<AuthResult> task = auth.SignInWithCustomTokenAsync(token);
            try
            {
                await task.AsUniTask().AttachExternalCancellation(cancellationToken);
                return task.Result.User;
            }
            catch
            {
                throw new ErrorDataException("Authorization Error", task.Exception.Message, canProceed: true, mustSignOut: true);
            }
#endif
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        static void AuthStateChanged(bool signedIn, string userId)
        {
            if(IsAutorized != signedIn)
            {
                if (!signedIn)
                {
                    IsAutorized = false;
                    logger.Log("Signed out " + userId);
                    OnSignOut?.Invoke();
                }
                if (signedIn)
                {
                    IsAutorized = true;
                    logger.Log("Signed in " + userId);
                    OnSignIn?.Invoke(userId);
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
            if (auth.CurrentUser != user)
            {
                bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                    && auth.CurrentUser.IsValid();
                if (!signedIn && user != null)
                {
                    logger.Log("Signed out " + user.UserId);
                    OnSignOut?.Invoke();
                }
                user = auth.CurrentUser;
                if (signedIn)
                {
                    logger.Log("Signed in " + user.UserId);
                    OnSignIn?.Invoke(user.UserId);
                }
            }
        }
        public static void SignOut() => auth.SignOut();
#endif
    }
}