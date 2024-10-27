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
using Energy8.Models.Requests;
using static Energy8.Requests.RequestsController;

namespace Energy8.Auth
{
    public class AuthControllerBase : MonoBehaviour
    {
        [Header("Logger")]
        [SerializeField] string loggerName = "AuthController";
        [SerializeField] Color loggerColor = Color.red;

        [Header("Content (Base)")]
        [SerializeField] List<AuthContentBase> contentPrefabs;

        [Header("Functional (Base)")]
        [SerializeField] ScrollRect scrollView;
        [SerializeField] RectTransform viewport;
        [SerializeField] new Animation animation;

        [Header("UI (Base)")]
        [SerializeField] Button openBut;

        [Header("Animations (Base)")]
        [SerializeField] string openClipName = "Open";
        [SerializeField] string closeClipName = "Close";

        private protected Logger logger;

        public static AuthControllerBase Instance { get; private set; }

        public string AuthToken { get; private set; }

        public bool IsOpen { get; private protected set; } = false;
        public bool IsDetailedAnalyticsAllowed { get; private protected set; } = false;
        public bool IsInitialized { get; private set; } = false;

        private protected CancellationTokenSource onSignInCTS;
        private protected CancellationTokenSource onSignOutCTS;

        public event Action<UserData> OnSignIn;
        public event Action OnSignOut;

        public UserData User { get; private set; }

        void ThrowCriticalError()
        {
            AuthController.SignOut();
            throw new Exception("Critical Authorization error");
        }

        #region Unity
        void Reset()
        {
            transform.Find("Scroll View").TryGetComponent(out scrollView);
            scrollView?.TryGetComponent(out animation);

            scrollView?.transform.Find("Viewport").TryGetComponent(out viewport);

            scrollView?.transform.Find("OpenBut").TryGetComponent(out openBut);
        }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                logger = new(this, loggerName, loggerColor);

                InitializeEvents();
                InitializeButtons();

                logger.Log($"Application information: {Application.companyName} {Application.productName}:{Application.version}");

                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        void Start()
        {
            onSignInCTS = new();
            StartAuthorizationAsync(onSignInCTS.Token).Forget();
        }

        void OnDestroy()
        {
            onSignInCTS?.Cancel();
            onSignOutCTS?.Cancel();
        }
        #endregion

        #region UI
        public void SetOpenState(bool isOpen)
        {
            IsOpen = isOpen;
            animation.Play(isOpen ? openClipName : closeClipName);
        }
        #endregion

        #region Authorization
        async UniTask StartAuthorizationAsync(CancellationToken cancellationToken)
        {
            logger.Log("StartAuthorizationAsync()");

            await FirebaseContoller.InitializeAllAsync(cancellationToken);
            IsDetailedAnalyticsAllowed = await RequestDetailedAnalyticsAsync(cancellationToken);
            IsInitialized = true;

            if (!AuthController.IsAutorized)
                await SignInAsync(cancellationToken);
        }
        async UniTask SignInAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryResult<string> authResult;
            do
            {
                SignInContentResult signInResult = (await AddAndProcessContentAsync<SignInContent, SignInContentResult>(cancellationToken)).Value;
                authResult = await RunWithHandlingError(cancellationToken, async () =>
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
            while (!authResult.IsSuccessful);
        }

        #region Email
        async UniTask<TryResult<string>> SignInByEmailAsync(CancellationToken cancellationToken, string email)
        {
            var confTokenResult = await SendSignInByEmailAsync(cancellationToken, email);
            if (confTokenResult.IsSuccessful)
            {
                var authTokenResult = await ConfirmEmailByCodeAsync(cancellationToken, email, confTokenResult.Value.Token);
                if (authTokenResult.IsSuccessful)
                    return await SignInByCustomToken(cancellationToken, authTokenResult.Value.AuthToken);
                else if (authTokenResult.IsCancelled)
                    return TryResult<string>.CreateCancelled();
                else return TryResult<string>.CreateFailed(authTokenResult.Error);
            }
            else return TryResult<string>.CreateFailed(confTokenResult.Error);
        }

        async UniTask<TryResult<ConfirmEmailByCodeResponseData>> ConfirmEmailByCodeAsync(CancellationToken cancellationToken, string email, string token)
        {
            TryResult<ConfirmEmailByCodeResponseData> authTokenResult;
            do
            {
                authTokenResult = await RunWithHandlingError(cancellationToken, async () =>
                {
                    var codeContentResult = await AddAndProcessContentAsync<CodeContent, CodeContentResult>(cancellationToken); ;
                    if (codeContentResult.IsSuccessful)
                        return await SendConfirmEmailByCodeAsync(cancellationToken, email.ToLower(), token, codeContentResult.Value.Code);
                    else if (codeContentResult.IsCancelled)
                        return TryResult<ConfirmEmailByCodeResponseData>.CreateCancelled();
                    else return TryResult<ConfirmEmailByCodeResponseData>.CreateFailed(codeContentResult.Error);
                });
            }
            while (authTokenResult.IsFailed);
            return authTokenResult;
        }
        async UniTask<TryResult<string>> SignInByCustomToken(CancellationToken cancellationToken, string token)
        {
            Func<UniTask<object>> authRequest = async () =>
                await AuthController.SignInByTokenAsync(cancellationToken, token);
            var authResult = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Simple, authRequest);
            if (authResult.IsSuccessful)
                return (TryResult<string>)authResult.Value.ObjectResult;
            else return TryResult<string>.CreateFailed(authResult.Error);
        }

        async UniTask<TryResult<SignInByEmailResponseData>> SendSignInByEmailAsync(
            CancellationToken cancellationToken, string email) =>
                await SendRequestAsync<SignInByEmailResponseData>(
                    cancellationToken, "SignInByEmail", "User/SignInByEmail", POST_METHOD, AuthorizationType.None, null, false, ("Email", email));
        async UniTask<TryResult<ConfirmEmailByCodeResponseData>> SendConfirmEmailByCodeAsync(
            CancellationToken cancellationToken, string email, string token, string code) =>
                await SendRequestAsync<ConfirmEmailByCodeRequestData, ConfirmEmailByCodeResponseData>(
                    cancellationToken, "ConfirmEmailByCode", "User/ConfirmSignInByCode", POST_METHOD, AuthorizationType.None,
                        null, new ConfirmEmailByCodeRequestData(email, token, code));
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
        async UniTask ShowUserWindowAsync(CancellationToken cancellationToken)
        {
            logger.Log("ShowUserWindowAsync()");

            await UniTask.WaitUntil(() => IsInitialized).
                AttachExternalCancellation(cancellationToken);

            do
            {
                await GetUserAsync(cancellationToken);
                await AddAndProcessUserContentAsync(cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }

        async UniTask GetUserAsync(CancellationToken cancellationToken)
        {
            var getUserResult = await RunWithHandlingError(cancellationToken, async () =>
            {
                var result = await SendGetUserRequestAsync(cancellationToken);
                if (result.IsFailed)
                    result = new TryResult<UserData>(result.Status, result.Value,
                        new ErrorData(result.Error.Header, result.Error.Description, canRetry: true, mustSignOut: true));
                return result;
            });

            if (getUserResult.IsSuccessful)
            {
                User = getUserResult.Value;
                OnSignIn?.Invoke(User);
            }
            else ThrowCriticalError();
        }

        async UniTask AddAndProcessUserContentAsync(CancellationToken cancellationToken)
        {
            var userResult = await AddAndProcessContentAsync<UserContent, UserContentResult>(cancellationToken, User.Name);
            if (userResult.Value.ResultType == UserWindowAction.SignOut)
                AuthController.SignOut();
            else
                await AddAndProcessChangeNameWindow(cancellationToken);
        }
        async UniTask AddAndProcessChangeNameWindow(CancellationToken cancellationToken)
        {
            TryResult changeNameResult =
                await RunWithHandlingError(cancellationToken, async () =>
                    {
                        var nameResult = await AddAndProcessContentAsync<ChangeNameContent, ChangeNameContentResult>(cancellationToken);
                        if (nameResult.IsSuccessful)
                            return await SendChangeNameRequestAsync(cancellationToken, nameResult.Value.Name);
                        else if (nameResult.IsCancelled)
                            return TryResult.CreateCancelled();
                        else return TryResult.CreateFailed(nameResult.Error);
                    });
            if (changeNameResult.IsFailed) ThrowCriticalError();
        }

        async UniTask<TryResult<UserData>> SendGetUserRequestAsync(CancellationToken cancellationToken) =>
            await SendRequestAsync<UserData>(
                cancellationToken, "GetUser", "User/GetUser", GET_METHOD, AuthorizationType.Bearer, () => AuthToken);
        async UniTask<TryResult> SendChangeNameRequestAsync(CancellationToken cancellationToken, string name) =>
                await SendRequestAsync(cancellationToken, "ChangeName", "User/ChangeName", PUT_METHOD, AuthorizationType.Bearer,
                    () => AuthToken, false, ("Name", name));
        #endregion

        async UniTask<bool> RequestDetailedAnalyticsAsync(CancellationToken cancellationToken)
        {
            if (PlayerPrefs.HasKey("IsDetailedAnalytics"))
                return bool.Parse(PlayerPrefs.GetString("IsDetailedAnalytics"));
            var result = await AddAndProcessContentAsync<AnalyticsContent, AnalyticsContentResult>(cancellationToken);
            PlayerPrefs.SetString("IsDetailedAnalytics", result.Value.IsDetailedAnalyticsAllowed.ToString());
            return result.Value.IsDetailedAnalyticsAllowed;
        }
        #endregion

        #region Functionaly
        async UniTask<TryResult<TResult>> RunWithHandlingError<TResult>(CancellationToken cancellationToken, Func<UniTask<TryResult<TResult>>> func)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TryResult<TResult> result;
            while (true)
            {
                result = await UniTask.Create(func);
                if (result.IsFailed)
                {
                    var errorResultValue = (await AddAndProcessContentAsync<ErrorContent, ErrorContentResult>(cancellationToken, result.Error)).Value;
                    switch (errorResultValue.Method)
                    {
                        case ErrorHandlingMethod.Close:
                            return TryResult<TResult>.CreateCancelled();
                        case ErrorHandlingMethod.TryAgain:
                            break;
                        case ErrorHandlingMethod.SignOut:
                            AuthController.SignOut();
                            return result;
                    }
                }
                else return result;
            }
        }
        async UniTask<TryResult> RunWithHandlingError(CancellationToken cancellationToken, Func<UniTask<TryResult>> func) =>
            await RunWithHandlingError(cancellationToken, async () => await func());

        protected async UniTask<TryResult<TResponse>> SendRequestAsync<TRequest, TResponse>(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, TRequest requestData, bool isBackground = false)
                        where TRequest : Data
                        where TResponse : Data
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte attemp = 1;
            while (attemp < 3)
            {
                string authData = getAuthData != null ? getAuthData() : string.Empty;
                var task = method.ToUpper() switch
                {
                    GET_METHOD => Get<TResponse>(endpoint, authType, authData, requestData),
                    PUT_METHOD => Put<TResponse>(endpoint, authType, authData, requestData),
                    DELETE_METHOD => Delete<TResponse>(endpoint, authType, authData, requestData),
                    _ => Post<TResponse>(endpoint, authType, authData, requestData),
                };
                Func<UniTask<WebTryResult<Data>>> request = async () =>
                    WebTryResult<Data>.Create<TResponse, Data>(await task);

                WebTryResult<Data> webResult;

                if (isBackground)
                    webResult = await UniTask.Create(request);
                else
                {
                    var result = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.WebRequest, request);
                    webResult = result.Value.RequestResult;
                }

                if (webResult.IsSuccessful)
                {
                    logger.Log($"{requestName}: {(TResponse)webResult.Value}");
                    return TryResult<TResponse>.CreateSuccessful((TResponse)webResult.Value);
                }
                else
                {
                    if (webResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask<object>> tokenRequest = async () =>
                            await UpdateAuthTokenAsync(cancellationToken, true);
                        var tokenResult = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                            LoadingContentType.Simple, tokenRequest);
                        if (tokenResult.IsSuccessful)
                        {
                            attemp++;
                            continue;
                        }
                        else
                        {
                            return TryResult<TResponse>.CreateFailed(new ErrorData("Unable to obtain authorization token",
                                "Please try restarting the app and signing in again. If this doesn't help, contact support.",
                                canProceed: true, canRetry: true, mustSignOut: true));
                        }
                    }
                    logger.LogWarning($"{requestName}: {webResult.Error}");
                    return TryResult<TResponse>.CreateFailed(webResult.Error);
                }
            }
            return TryResult<TResponse>.CreateFailed(new ErrorData("Authorization Error",
                "Please try restarting the app and signing in again. If this doesn't help, contact support.", canProceed: true, canRetry: true, mustSignOut: true));
        }
        protected async UniTask<TryResult<TResponse>> SendRequestAsync<TResponse>(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, string value)[] requestData)
            where TResponse : Data
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte attemp = 1;
            while (attemp < 3)
            {
                string authData = getAuthData != null ? getAuthData() : string.Empty;
                var task = method.ToUpper() switch
                {
                    GET_METHOD => Get<TResponse>(endpoint, authType, authData, requestData),
                    PUT_METHOD => Put<TResponse>(endpoint, authType, authData, requestData),
                    DELETE_METHOD => Delete<TResponse>(endpoint, authType, authData, requestData),
                    _ => Post<TResponse>(endpoint, authType, authData, requestData),
                };
                Func<UniTask<WebTryResult<Data>>> request = async () =>
                    WebTryResult<Data>.Create<TResponse, Data>(await task);

                WebTryResult<Data> webResult;

                if (isBackground)
                    webResult = await UniTask.Create(request);
                else
                {
                    var result = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.WebRequest, request);
                    webResult = result.Value.RequestResult;
                }

                if (webResult.IsSuccessful)
                {
                    logger.Log($"{requestName}: {(TResponse)webResult.Value}");
                    return TryResult<TResponse>.CreateSuccessful((TResponse)webResult.Value);
                }
                else
                {
                    if (webResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask<object>> tokenRequest = async () =>
                            await UpdateAuthTokenAsync(cancellationToken, true);
                        var tokenResult = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                            LoadingContentType.Simple, tokenRequest);
                        if (tokenResult.IsSuccessful)
                        {
                            attemp++;
                            continue;
                        }
                        else
                        {
                            return TryResult<TResponse>.CreateFailed(new ErrorData("Unable to obtain authorization token",
                                "Please try restarting the app and signing in again. If this doesn't help, contact support.",
                                canProceed: true, canRetry: true, mustSignOut: true));
                        }
                    }
                    logger.LogWarning($"{requestName}: {webResult.Error}");
                    return TryResult<TResponse>.CreateFailed(webResult.Error);
                }
            }
            return TryResult<TResponse>.CreateFailed(new ErrorData("Authorization Error",
                "Please try restarting the app and signing in again. If this doesn't help, contact support.", canProceed: true, canRetry: true, mustSignOut: true));
        }
        protected async UniTask<TryResult> SendRequestAsync(CancellationToken cancellationToken,
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, string value)[] requestData)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte attemp = 1;
            while (attemp < 3)
            {
                string authData = getAuthData != null ? getAuthData() : string.Empty;
                var task = method.ToUpper() switch
                {
                    GET_METHOD => Get(endpoint, authType, authData, requestData),
                    PUT_METHOD => Put(endpoint, authType, authData, requestData),
                    DELETE_METHOD => Delete(endpoint, authType, authData, requestData),
                    _ => Post(endpoint, authType, authData, requestData),
                };
                Func<UniTask<object>> request = async () => await task;

                WebTryResult webResult;

                if (isBackground)
                    webResult = (await UniTask.Create(request)) as WebTryResult;
                else
                {
                    var result = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken, LoadingContentType.Simple, request);
                    webResult = result.Value.ObjectResult as WebTryResult;
                }

                if (webResult.IsSuccessful)
                {
                    logger.Log($"{requestName}");
                    return TryResult.CreateSuccessful();
                }
                else
                {
                    if (webResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Func<UniTask<object>> tokenRequest = async () =>
                            await UpdateAuthTokenAsync(cancellationToken, true);
                        var tokenResult = await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                            LoadingContentType.Simple, tokenRequest);
                        if (tokenResult.IsSuccessful)
                        {
                            attemp++;
                            continue;
                        }
                        else
                        {
                            return TryResult.CreateFailed(new ErrorData("Unable to obtain authorization token",
                                "Please try restarting the app and signing in again. If this doesn't help, contact support.",
                                canProceed: true, canRetry: true, mustSignOut: true));
                        }
                    }
                    logger.LogWarning($"{requestName}: {webResult.Error}");
                    return TryResult.CreateFailed(webResult.Error);
                }
            }
            return TryResult.CreateFailed(new ErrorData("Authorization Error",
                "Please try restarting the app and signing in again. If this doesn't help, contact support.", canProceed: true, canRetry: true, mustSignOut: true));
        }

        async UniTask<TryResult<string>> UpdateAuthTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
            var result = await AuthController.GetAuthTokenAsync(cancellationToken, forceRefresh);
            if (result.IsSuccessful)
                AuthToken = result.Value;
            return result;
        }

        async UniTask<TryResult<TResult>> AddAndProcessContentAsync<TContent, TResult>(CancellationToken cancellationToken, params object[] args)
            where TContent : AuthContentBase
            where TResult : AuthContentResultBase
        {
            cancellationToken.ThrowIfCancellationRequested();
            TContent contentPrefab = null;

            foreach (var c in contentPrefabs)
                if (c.GetType() == typeof(TContent))
                    contentPrefab = (TContent)c;

            if (contentPrefab == null)
                throw new PrefabNotFoundException(typeof(TContent));

            TContent content = Instantiate(contentPrefab, viewport);
            scrollView.content = content.RectTransform;
            return await content.TryProcessContentAsync<TResult>(cancellationToken, args);
        }
        #endregion

        #region Initialization
        private protected virtual void InitializeEvents()
        {
            AuthController.OnSignIn += (_) =>
            {
                onSignInCTS?.Cancel();
                onSignOutCTS = new();
                ShowUserWindowAsync(onSignOutCTS.Token).Forget();
            };
            AuthController.OnSignOut += () =>
            {
                OnSignOut?.Invoke();
                onSignOutCTS?.Cancel();
                onSignInCTS = new();
                SignInAsync(onSignInCTS.Token).Forget();
            };
        }
        private protected virtual void InitializeButtons()
        {
            openBut.onClick.AddListener(() => SetOpenState(!IsOpen));
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