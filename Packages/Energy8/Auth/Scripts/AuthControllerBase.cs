using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Auth.Content;
using System;
using Energy8.Firebase;
using Energy8.Models.User;
using Energy8.Models.SignIn;
using Energy8.Models;
using System.Net;
using Energy8.Models.Errors;
using static Energy8.Requests.RequestsController;
using System.Linq;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Models.WebGL.Firebase;
#else
using Firebase.Auth;
#endif

namespace Energy8.Auth
{
    public class AuthControllerBase : MonoBehaviour
    {
        [Header("Logger")]
        [SerializeField] string _loggerName = "AuthController";
        [SerializeField] Color _loggerColor = Color.red;

        [Header("Content (Base)")]
        [SerializeField] List<AuthContentBase> _contentPrefabs;

        [Header("Functional (Base)")]
        [SerializeField] ScrollRect _scrollView;
        [SerializeField] RectTransform _viewport;
        [SerializeField] Animation _animation;

        [Header("UI (Base)")]
        [SerializeField] Button _openBut;

        [Header("Animations (Base)")]
        [SerializeField] string _openClipName = "Open";
        [SerializeField] string _closeClipName = "Close";

        private protected Logger _logger;

        public static AuthControllerBase Instance { get; private set; }

        public UserData User { get; private set; }
        public string AuthToken { get; private set; }

        public bool IsOpen { get; private protected set; } = false;
        public bool IsDetailedAnalyticsAllowed { get; private protected set; } = false;
        public bool IsInitialized { get; private set; } = false;

        private protected CancellationTokenSource _onSignInCTS;
        private protected CancellationTokenSource _onSignOutCTS;

        public event Action<UserData> OnSignInEvent;
        public event Action OnSignOutEvent;

        void ThrowCriticalError()
        {
            AuthController.SignOut();
            throw new Exception("Critical Authorization error");
        }

        #region Unity
        void Reset()
        {
            if (transform.Find("Scroll View").TryGetComponent(out _scrollView))
            {
                _scrollView.TryGetComponent(out _animation);
                _scrollView.transform.Find("Viewport").TryGetComponent(out _viewport);
                _scrollView.transform.Find("OpenBut").TryGetComponent(out _openBut);
            }
        }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                _logger = new(this, _loggerName, _loggerColor);

                InitializeEvents();
                InitializeButtons();

                _logger.Log($"Application information: {Application.companyName} {Application.productName}:{Application.version}");

                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        void Start()
        {
            _onSignInCTS = new();
            StartAuthorizationAsync(_onSignInCTS.Token).Forget();
        }

        void OnDestroy()
        {
            _onSignInCTS?.Cancel();
            _onSignOutCTS?.Cancel();
        }
        #endregion

        #region UI
        public void SetOpenState(bool isOpen)
        {
            IsOpen = isOpen;
            _animation.Play(isOpen ? _openClipName : _closeClipName);
        }
        #endregion

        #region Authorization
        async UniTask StartAuthorizationAsync(CancellationToken cancellationToken)
        {
            _logger.Log("StartAuthorizationAsync()");

            await FirebaseController.InitializeAllAsync(cancellationToken);
            IsDetailedAnalyticsAllowed = await RequestDetailedAnalyticsAsync(cancellationToken);
            IsInitialized = true;

            if (!AuthController.IsAutorized)
                await SignInAsync(cancellationToken);
        }
        async UniTask SignInAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FirebaseUser authUser;
            while (true)
            {
                SignInContentResult signInResult = await AddAndProcessContentAsync<SignInContent, SignInContentResult>(cancellationToken);

                RunWithHandlingErrorStatus status;
                (status, authUser) = await RunWithHandlingError(cancellationToken, async () =>
                {
                    return signInResult.SignInMethod switch
                    {
                        SignInMethod.Email => await SignInByEmailAsync(cancellationToken, signInResult.Email),
                        SignInMethod.Google => null,
                        SignInMethod.Apple => null,
                        _ => null,
                    };
                });
            }
        }

        #region Email
        async UniTask<FirebaseUser> SignInByEmailAsync(CancellationToken cancellationToken, string email)
        {
            _logger.Log($"SignInByEmailAsync({email})");
            var confTokenResult = await SendSignInByEmailAsync(cancellationToken, email);
            var authTokenResult = await ConfirmEmailByCodeAsync(cancellationToken, email, confTokenResult.Token);
            return await SignInByCustomToken(cancellationToken, authTokenResult.AuthToken);
        }

        async UniTask<ConfirmSignInResponseData> ConfirmEmailByCodeAsync(CancellationToken cancellationToken, string email, string token)
        {
            ConfirmSignInResponseData authTokenResult;
            RunWithHandlingErrorStatus status;

            do
            {
                (status, authTokenResult) = await RunWithHandlingError(cancellationToken, async () =>
                {
                    var codeContentResult = await AddAndProcessContentAsync<CodeContent, CodeContentResult>(cancellationToken);
                    return await SendConfirmSignInAsync(cancellationToken, email.ToLower(), token, codeContentResult.Code);
                });
            }
            while (status == RunWithHandlingErrorStatus.Cancelled);

            return authTokenResult;
        }
        async UniTask<FirebaseUser> SignInByCustomToken(CancellationToken cancellationToken, string token)
        {
            async UniTask<object> sendAuthRequest() =>
                await AuthController.SignInByTokenAsync(cancellationToken, token);

            return (FirebaseUser)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Object, (Func<UniTask<object>>)sendAuthRequest)).ObjectResult;
        }

        async UniTask<SignInByEmailResponseData> SendSignInByEmailAsync(
            CancellationToken cancellationToken, string email) =>
                await SendRequestAsync<SignInByEmailResponseData>(
                    cancellationToken, "SignInByEmail", "User/SignInByEmail", PostMethod, AuthorizationType.None, null, false, ("Email", email));

        async UniTask<ConfirmSignInResponseData> SendConfirmSignInAsync(
            CancellationToken cancellationToken, string email, string token, string code) =>
                await SendRequestAsync<EmailCodeRequestData, ConfirmSignInResponseData>(
                    cancellationToken, "ConfirmSignInByEmail", "User/ConfirmSignInByEmail", PostMethod, AuthorizationType.None,
                        null, new EmailCodeRequestData(email, token, code));
        #endregion

        #region Google
        async UniTask<string> GetGoogleIdTokenAsync(CancellationToken cancellationToken)
        {
            UniTaskCompletionSource<string> gsiSource = new();
            GoogleSignIn.SignIn("523375396079-8emtlq3ran6sobr9uakfn3ohvkjc0g8e.apps.googleusercontent.com",
                (token) => gsiSource.TrySetResult(token),
                (error) => throw new Exception(error)
            );
            return await gsiSource.Task.AttachExternalCancellation(cancellationToken);
        }
        #endregion

        #region  UserWindow
        private protected virtual async UniTask ShowUserWindowAsync(CancellationToken cancellationToken)
        {
            _logger.Log("ShowUserWindowAsync()");

            await UniTask.WaitUntil(() => IsInitialized).
                AttachExternalCancellation(cancellationToken);

            do
            {
                await GetUserAsync(cancellationToken);
                await AddAndProcessUserContentAsync(cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }

        private protected async UniTask GetUserAsync(CancellationToken cancellationToken)
        {
            UserData userData;
            RunWithHandlingErrorStatus status;

            (status, userData) = await RunWithHandlingError(cancellationToken, async () =>
            {
                return await SendGetUserRequestAsync(cancellationToken);
            });

            if (status == RunWithHandlingErrorStatus.Successful)
            {
                User = userData;
                OnSignInEvent?.Invoke(User);
            }
            else ThrowCriticalError();
        }

        private protected async UniTask AddAndProcessUserContentAsync(CancellationToken cancellationToken)
        {
            var userResult = await AddAndProcessContentAsync<UserContent, UserContentResult>(cancellationToken, User.Name);

            if (userResult.ResultType == UserWindowAction.SignOut)
                AuthController.SignOut();
            else
            {
                var status = await AddAndProcessChangeNameWindow(cancellationToken);
                if (status == RunWithHandlingErrorStatus.SignedOut)
                    AuthController.SignOut();
            }
        }
        async UniTask<RunWithHandlingErrorStatus> AddAndProcessChangeNameWindow(CancellationToken cancellationToken)
        {
            return await RunWithHandlingError(cancellationToken, async () =>
                    {
                        var changeNameContentResult = await AddAndProcessContentAsync<ChangeNameContent, ChangeNameContentResult>(cancellationToken);
                        await SendChangeNameRequestAsync(cancellationToken, changeNameContentResult.Name);
                    });
        }

        async UniTask<UserData> SendGetUserRequestAsync(CancellationToken cancellationToken) =>
            await SendRequestAsync<UserData>(
                cancellationToken, "GetUser", "User/GetUser", GetMethod, AuthorizationType.Bearer, () => AuthToken);
        async UniTask SendChangeNameRequestAsync(CancellationToken cancellationToken, string name) =>
                await SendRequestAsync(cancellationToken, "ChangeName", "User/ChangeName", PutMethod, AuthorizationType.Bearer,
                    () => AuthToken, false, ("Name", name));
        #endregion

        async UniTask<bool> RequestDetailedAnalyticsAsync(CancellationToken cancellationToken)
        {
            if (PlayerPrefs.HasKey("IsDetailedAnalytics"))
                return bool.Parse(PlayerPrefs.GetString("IsDetailedAnalytics"));
            var result = await AddAndProcessContentAsync<AnalyticsContent, AnalyticsContentResult>(cancellationToken);
            PlayerPrefs.SetString("IsDetailedAnalytics", result.IsDetailedAnalyticsAllowed.ToString());
            return result.IsDetailedAnalyticsAllowed;
        }
        #endregion

        #region Functionaly
        private protected enum RunWithHandlingErrorStatus
        {
            Successful,
            Cancelled,
            SignedOut
        }

        private protected async UniTask<(RunWithHandlingErrorStatus Status, TResult Result)> RunWithHandlingError<TResult>(CancellationToken cancellationToken, Func<UniTask<TResult>> func)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (true)
            {
                try
                {
                    return (RunWithHandlingErrorStatus.Successful, await UniTask.Create(func));
                }
                catch (Exception ex)
                {
                    ErrorData error;
                    if (typeof(ErrorDataException) == ex.GetType())
                        error = (ex as ErrorDataException).Error;
                    else
                        error = new ErrorData("Unknown Error", ex.Message, canProceed: true, canRetry: true, mustSignOut: true);
                    var errorResultValue = await AddAndProcessContentAsync<ErrorContent, ErrorContentResult>(cancellationToken, error);
                    switch (errorResultValue.Method)
                    {
                        case ErrorHandlingMethod.Close:
                            return (RunWithHandlingErrorStatus.Cancelled, default);
                        case ErrorHandlingMethod.TryAgain:
                            break;
                        case ErrorHandlingMethod.SignOut:
                            return (RunWithHandlingErrorStatus.SignedOut, default);
                    }
                }
            }
        }
        private protected async UniTask<RunWithHandlingErrorStatus> RunWithHandlingError(CancellationToken cancellationToken, Func<UniTask> func) =>
            await RunWithHandlingError(cancellationToken, async () => await func());

        protected async UniTask<TResponse> SendRequestAsync<TRequest, TResponse>(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, TRequest requestData, bool isBackground = false)
                        where TRequest : Data
                        where TResponse : Data
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Log($"SendRequestAsync({requestName}, {endpoint}, {method}, {authType}, {isBackground}, {requestData})");
            byte attemp = 1;
            Exception exception = null;
            while (attemp < 3)
            {
                string authData = getAuthData != null ? getAuthData() : string.Empty;
                var task = method.ToUpper() switch
                {
                    GetMethod => Get<TResponse>(endpoint, authType, authData, requestData.ToDictionary().ToList().Select((el) => (el.Key, el.Value)).ToArray()),
                    PutMethod => Put<TResponse>(endpoint, authType, authData, requestData),
                    DeleteMethod => Delete<TResponse>(endpoint, authType, authData, requestData),
                    _ => Post<TResponse>(endpoint, authType, authData, requestData),
                };
                Func<UniTask<Data>> request = async () => await task;

                try
                {
                    _logger.Log($"{requestName}");
                    if (isBackground)
                        return (TResponse)await UniTask.Create(request);
                    else
                        return (TResponse)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>
                            (cancellationToken, LoadingContentType.WebRequest, request)).RequestResult;
                }
                catch (RequestErrorDataException ex)
                {
                    exception = ex;
                    if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask> tokenRequest = async () => await UpdateAuthTokenAsync(cancellationToken, true);
                        await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.Empty, tokenRequest);
                        attemp++;
                        continue;
                    }
                    _logger.LogWarning($"{requestName}: {ex.Error}");
                    throw ex;
                }
            }
            throw exception;
        }
        protected async UniTask<TResponse> SendRequestAsync<TResponse>(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, string value)[] requestDataFields)
            where TResponse : Data
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Log($"SendRequestAsync({requestName}, {endpoint}, {method}, {authType}, {isBackground}, {string.Join(" ", requestDataFields)})");
            byte attemp = 1;
            RequestErrorDataException exception = null;
            while (attemp < 3)
            {
                string authData = authType == AuthorizationType.None ? string.Empty : getAuthData();
                var task = method.ToUpper() switch
                {
                    GetMethod => Get<TResponse>(endpoint, authType, authData, headers: requestDataFields),
                    PutMethod => Put<TResponse>(endpoint, authType, authData, requestDataFields: requestDataFields),
                    DeleteMethod => Delete<TResponse>(endpoint, authType, authData, requestDataFields: requestDataFields),
                    _ => Post<TResponse>(endpoint, authType, authData, requestDataFields: requestDataFields)
                };

                Func<UniTask<Data>> request = async () => await task;

                try
                {
                    _logger.Log($"{requestName}");
                    if (isBackground)
                        return (TResponse)await UniTask.Create(request);
                    else
                        return (TResponse)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>
                            (cancellationToken, LoadingContentType.WebRequest, request)).RequestResult;
                }
                catch (RequestErrorDataException ex)
                {
                    exception = ex;
                    if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask> tokenRequest = async () => await UpdateAuthTokenAsync(cancellationToken, true);
                        await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.Empty, tokenRequest);
                        attemp++;
                        continue;
                    }
                    _logger.LogWarning($"{requestName}: {ex.Error}");
                    throw ex;
                }
            }
            throw exception;
        }
        protected async UniTask SendRequestAsync(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, string value)[] requestDataFields)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.Log($"SendRequestAsync({requestName}, {endpoint}, {method}, {authType}, {isBackground}, {string.Join(" ", requestDataFields)})");
            byte attemp = 1;
            RequestErrorDataException exception = null;
            while (attemp < 3)
            {
                string authData = authType == AuthorizationType.None ? string.Empty : getAuthData();

                var task = method.ToUpper() switch
                {
                    GetMethod => Get(endpoint, authType, authData, requestDataFields),
                    PutMethod => Put(endpoint, authType, authData, requestDataFields: requestDataFields),
                    DeleteMethod => Delete(endpoint, authType, authData, requestDataFields: requestDataFields),
                    _ => Post(endpoint, authType, authData, requestDataFields: requestDataFields),
                };

                Func<UniTask> request = async () => await task;

                try
                {
                    if (isBackground)
                        await UniTask.Create(request);
                    else
                        await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.Empty, request);

                    _logger.Log($"{requestName}");
                }
                catch (RequestErrorDataException ex)
                {
                    exception = ex;
                    if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask> tokenRequest = async () => await UpdateAuthTokenAsync(cancellationToken, true);
                        await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.Empty, tokenRequest);
                        attemp++;
                        continue;
                    }
                    _logger.LogWarning($"{requestName}: {ex.Error}");
                    throw ex;
                }
            }
            throw exception;
        }

        async UniTask UpdateAuthTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
            AuthToken = await AuthController.GetAuthTokenAsync(cancellationToken, forceRefresh);
        }

        async UniTask<TResult> AddAndProcessContentAsync<TContent, TResult>(CancellationToken cancellationToken, params object[] args)
            where TContent : AuthContentBase
            where TResult : AuthContentResultBase
        {
            _logger.Log($"AddAndProcessContentAsync({typeof(TContent)}, {string.Join(", ", args)})");
            cancellationToken.ThrowIfCancellationRequested();
            TContent contentPrefab = null;

            foreach (var c in _contentPrefabs)
                if (c.GetType() == typeof(TContent))
                    contentPrefab = (TContent)c;

            if (contentPrefab == null)
                throw new PrefabNotFoundException(typeof(TContent));

            TContent content = Instantiate(contentPrefab, _viewport);
            _scrollView.content = content.RectTransform;
            return await content.TryProcessContentAsync<TResult>(cancellationToken, args);
        }
        #endregion

        #region Initialization
        private protected virtual void InitializeEvents()
        {
            AuthController.OnSignInEvent += (_) =>
            {
                _onSignInCTS?.Cancel();
                _onSignOutCTS = new();
                ShowUserWindowAsync(_onSignOutCTS.Token).Forget();
            };
            AuthController.OnSignOutEvent += () =>
            {
                OnSignOutEvent?.Invoke();
                _onSignOutCTS?.Cancel();
                _onSignInCTS = new();
                SignInAsync(_onSignInCTS.Token).Forget();
            };
        }
        private protected virtual void InitializeButtons()
        {
            _openBut.onClick.AddListener(() => SetOpenState(!IsOpen));
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("SignOut")]
        private void SignOut()
        {
            AuthController.SignOut();
        }
#endif
    }

    public class PrefabNotFoundException : Exception
    {
        public Type PrefabType { get; }

        public PrefabNotFoundException()
            : base("Prefab not found.") { }

        public PrefabNotFoundException(Type prefabType)
            : base($"Prefab of type '{prefabType}' not found. Please ensure the prefab of this type exists in the project.")
        {
            PrefabType = prefabType;
        }

        public PrefabNotFoundException(Type prefabType, string message)
            : base(message)
        {
            PrefabType = prefabType;
        }

        public PrefabNotFoundException(Type prefabType, string message, Exception innerException)
            : base(message, innerException)
        {
            PrefabType = prefabType;
        }
    }
}