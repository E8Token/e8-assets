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
using Newtonsoft.Json;


#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Models.WebGL.Firebase;
using Energy8.Plugins.WebGL.LocalStorage;
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

        private protected CancellationTokenSource _onSignedInCTS;
        private protected CancellationTokenSource _onSignedOutCTS;

        public event Action<UserData> OnSignedIn;
        public event Action OnSignedOut;

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
                _logger.Log($"Application information: {Application.companyName} {Application.productName}:{Application.version}");

                InitializeEvents();
                InitializeUI();

                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        void Start()
        {
            _onSignedInCTS = new();
            StartSignInProcessAsync(_onSignedInCTS.Token).Forget();
        }
        void OnDestroy()
        {
            _onSignedInCTS?.Cancel();
            _onSignedOutCTS?.Cancel();
        }
        #endregion

        #region UI
        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;
            IsOpen = isOpen;
            _animation.Play(isOpen ? _openClipName : _closeClipName);
        }
        #endregion

        #region SignIn
        async UniTask StartSignInProcessAsync(CancellationToken cancellationToken)
        {
            _logger.Log("StartSignInProcessAsync()");

            await FirebaseController.InitializeAllAsync(cancellationToken);
            IsDetailedAnalyticsAllowed = await RequestDetailedAnalyticsAsync(cancellationToken);
            IsInitialized = true;

            if (!AuthController.IsSignedIn)
                await SignInAsync(cancellationToken);
        }
        async UniTask SignInAsync(CancellationToken cancellationToken)
        {
            _logger.Log("SignInAsync()");

            cancellationToken.ThrowIfCancellationRequested();
            FirebaseUser authUser;
#if !UNITY_WEBGL || UNITY_EDITOR
            SetOpenState(true);
#endif
            while (true)
            {
                SignInContentResult signInResult = await AddAndProcessContentAsync<SignInContent, SignInContentResult>(cancellationToken);

                RunWithHandlingErrorStatus status;
                (status, authUser) = await RunWithHandlingError(cancellationToken, async () =>
                {
                    return signInResult.SignInMethod switch
                    {
                        SignInMethod.Email => await SignInByEmailAsync(cancellationToken, signInResult.Email),
                        SignInMethod.Google => await SignInWithGoogleAsync(cancellationToken),
                        SignInMethod.Apple => await SignInWithAppleAsync(cancellationToken),
                        SignInMethod.Telegram => await SignInWithTelegramAsync(cancellationToken),
                        _ => null,
                    };
                });
            }
        }

        #region Email
        async UniTask<FirebaseUser> SignInByEmailAsync(CancellationToken cancellationToken, string email, bool addProvider = false)
        {
            _logger.Log($"SignInByEmailAsync({email})");
            var confTokenResult = await SendSignInByEmailAsync(cancellationToken, email);
            var authTokenResult = await ConfirmEmailByCodeAsync(cancellationToken, email, confTokenResult.Token, addProvider);
            return addProvider ? AuthController.User :
                await SignInByCustomToken(cancellationToken, authTokenResult.AuthToken);
        }

        async UniTask<ConfirmSignInResponseData> ConfirmEmailByCodeAsync(CancellationToken cancellationToken, string email, string token, bool addProvider)
        {
            _logger.Log($"ConfirmEmailByCodeAsync({email}, {token})");
            ConfirmSignInResponseData authTokenResult;
            RunWithHandlingErrorStatus status;

            do
            {
                (status, authTokenResult) = await RunWithHandlingError(cancellationToken, async () =>
                {
                    var codeContentResult = await AddAndProcessContentAsync<CodeContent, CodeContentResult>(cancellationToken);
                    string authId = addProvider ? AuthController.User.UserId : null;
                    return await SendConfirmSignInAsync(cancellationToken, token, codeContentResult.Code);
                });
            }
            while (status == RunWithHandlingErrorStatus.Cancelled);

            return authTokenResult;
        }

        async UniTask<FirebaseUser> SignInByCustomToken(CancellationToken cancellationToken, string token)
        {
            _logger.Log($"SignInByCustomToken({token})");
            async UniTask<object> sendAuthRequest() =>
                await AuthController.SignInWithTokenAsync(cancellationToken, token);

            return (FirebaseUser)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Object, (Func<UniTask<object>>)sendAuthRequest)).ObjectResult;
        }

        async UniTask<FirebaseUser> SignInWithGoogleAsync(CancellationToken cancellationToken, bool addProvider = false)
        {
            _logger.Log($"SignInWithGoogle()");
            async UniTask<object> sendAuthRequest() =>
                await AuthController.SignInWithGoogleAsync(cancellationToken, addProvider);

            return (FirebaseUser)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Object, (Func<UniTask<object>>)sendAuthRequest)).ObjectResult;
        }

        async UniTask<FirebaseUser> SignInWithAppleAsync(CancellationToken cancellationToken, bool addProvider = false)
        {
            _logger.Log($"SignInWithApple()");
            async UniTask<object> sendAuthRequest() =>
                await AuthController.SignInWithAppleAsync(cancellationToken, addProvider);

            return (FirebaseUser)(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Object, (Func<UniTask<object>>)sendAuthRequest)).ObjectResult;
        }

        async UniTask<FirebaseUser> SignInWithTelegramAsync(CancellationToken cancellationToken, bool addProvider = false)
        {
            _logger.Log($"SignInWithTelegram()");
            async UniTask<object> sendAuthRequest() =>
                await AuthController.SignInWithTelegramAsync(cancellationToken);

            (TelegramUserData user, string hash) = ((TelegramUserData, string))(await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(cancellationToken,
                LoadingContentType.Object, (Func<UniTask<object>>)sendAuthRequest)).ObjectResult;

            string authId = addProvider ? AuthController.User.UserId : null;

            var authTokenResult = await SendConfirmTelegramByHashAsync(cancellationToken, user, hash, authId);
            return addProvider ? AuthController.User :
                await SignInByCustomToken(cancellationToken, authTokenResult.AuthToken);
        }

        async UniTask<SignInWithEmailResponseData> SendSignInByEmailAsync(
            CancellationToken cancellationToken, string email) =>
                await SendRequestAsync<SignInWithEmailResponseData>(
                    cancellationToken, "SignInByEmail", "User/SignInWithEmail", PostMethod, AuthorizationType.None, null, false, ("Email", email));

        async UniTask<ConfirmSignInResponseData> SendConfirmSignInAsync(
            CancellationToken cancellationToken, string token, string code) =>
                await SendRequestAsync<EmailCodeRequestData, ConfirmSignInResponseData>(
                    cancellationToken, "ConfirmEmailWithCode", "User/ConfirmEmailWithCode", PostMethod, AuthorizationType.None,
                    null, new EmailCodeRequestData(token, code));
        async UniTask SendConfirmDeleteAccountAsync(
        CancellationToken cancellationToken, string token, string code) =>
            await SendRequestAsync(cancellationToken, "ConfirmEmailWithCode",
                "User/ConfirmEmailWithCode", DeleteMethod, AuthorizationType.Bearer,
                () => AuthToken, new EmailCodeRequestData(token, code));

        async UniTask<ConfirmSignInResponseData> SendConfirmTelegramByHashAsync(
        CancellationToken cancellationToken, TelegramUserData telegramUser, string hash, string authId = null) =>
            await SendRequestAsync<TelegramHashRequestData, ConfirmSignInResponseData>(
                cancellationToken, "ConfirmTelegramByHash", "User/ConfirmTelegramByHash", PostMethod, AuthorizationType.None,
                null, new TelegramHashRequestData(telegramUser, hash, authId));
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
        private async UniTask ShowUserContentAsync(CancellationToken cancellationToken)
        {
            // await UniTask.WaitUntil(() => IsInitialized).
            //     AttachExternalCancellation(cancellationToken);
            await UpdateAuthTokenAsync(cancellationToken, true);
            do
            {
                await UniTask.SwitchToMainThread();
                var status = await RunWithHandlingError(cancellationToken, async () =>
                {
                    await UpdateUserContentAsync(cancellationToken);
                });
                if (status == RunWithHandlingErrorStatus.SignedOut)
                {
                    AuthController.SignOut();
                    return;
                }
            } while (!cancellationToken.IsCancellationRequested);
        }

        protected virtual async UniTask UpdateUserContentAsync(CancellationToken cancellationToken)
        {
            await GetUserAsync(cancellationToken);
            await AddAndProcessUserContentAsync(cancellationToken);
        }

        private protected async UniTask GetUserAsync(CancellationToken cancellationToken)
        {
            User = await SendGetUserRequestAsync(cancellationToken);
        }

        private protected async UniTask AddAndProcessUserContentAsync(CancellationToken cancellationToken)
        {
            var userResult = await AddAndProcessContentAsync<UserContent, UserContentResult>(cancellationToken, User.Name);

            if (userResult.ResultType == UserWindowAction.SignOut)
                AuthController.SignOut();
            else if (userResult.ResultType == UserWindowAction.OpenSettings)
                await AddAndProcessSettingsContentAsync(cancellationToken);
        }

        async UniTask AddAndProcessDeleteAccountContent(CancellationToken cancellationToken)
        {
            await RunWithHandlingError(cancellationToken, async () =>
            {
                await AddAndProcessContentAsync<DeleteAccountContent, DeleteAccountContentResult>(cancellationToken);
                var response = await SendRequestAsync<DeleteAccountWithEmailResponseData>(cancellationToken, "DeleteAccount", "User/DeleteAccount", DeleteMethod, AuthorizationType.Bearer, () => AuthToken, false);
                var codeContentResult = await AddAndProcessContentAsync<CodeContent, CodeContentResult>(cancellationToken);
                await SendConfirmDeleteAccountAsync(cancellationToken, response.Token, codeContentResult.Code);
            });
        }
        async UniTask AddAndProcessChangeNameContent(CancellationToken cancellationToken)
        {
            await RunWithHandlingError(cancellationToken, async () =>
            {
                var changeNameContentResult = await AddAndProcessContentAsync<ChangeNameContent, ChangeNameContentResult>(cancellationToken);
                await SendChangeNameRequestAsync(cancellationToken, changeNameContentResult.Name);
            });
        }

        private protected async UniTask AddAndProcessSettingsContentAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var settingsResult = await AddAndProcessContentAsync<SettingsContent, SettingsContentResult>(cancellationToken,
                User.Name, AuthController.User.Email,
                User.AuthProviders.Contains("Google"),
                User.AuthProviders.Contains("Google"),
                User.AuthProviders.Contains("Google"));

                switch (settingsResult.ResultType)
                {
                    case SettingsWindowAction.ChangeName:
                        await AddAndProcessChangeNameContent(cancellationToken);
                        continue;

                    case SettingsWindowAction.DeleteAccount:
                        await AddAndProcessDeleteAccountContent(cancellationToken);
                        AuthController.SignOut();
                        return;

                    case SettingsWindowAction.AddGoogleProvider:
                        await SignInWithGoogleAsync(cancellationToken, true);
                        continue;

                    case SettingsWindowAction.AddAppleProvider:
                        await SignInWithAppleAsync(cancellationToken, true);
                        continue;

                    case SettingsWindowAction.AddTelegramProvider:
                        await SignInWithTelegramAsync(cancellationToken, true);
                        continue;

                    case SettingsWindowAction.AddEmailProvider:
                        await AddAndProcessChangeEmailContent(cancellationToken);
                        continue;

                    default:
                        return;
                }
            }
        }

        async UniTask AddAndProcessChangeEmailContent(CancellationToken cancellationToken, bool addProvider = true)
        {
            await RunWithHandlingError(cancellationToken, async () =>
            {
                var emailResult = await AddAndProcessContentAsync<ChangeEmailContent, ChangeEmailContentResult>(cancellationToken);
                var confTokenResult = await SendSignInByEmailAsync(cancellationToken, emailResult.Email);
                var authTokenResult = await ConfirmEmailByCodeAsync(cancellationToken, emailResult.Email, confTokenResult.Token, addProvider);
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
#if UNITY_WEBGL && !UNITY_EDITOR
            if (LocalStorageController.HasKey("IsDetailedAnalytics"))
                return bool.Parse(LocalStorageController.Get("IsDetailedAnalytics"));
#else
            if (PlayerPrefs.HasKey("IsDetailedAnalytics"))
                return bool.Parse(PlayerPrefs.GetString("IsDetailedAnalytics"));
#endif
            var result = await AddAndProcessContentAsync<AnalyticsContent, AnalyticsContentResult>(cancellationToken);
#if UNITY_WEBGL && !UNITY_EDITOR
            LocalStorageController.Set("IsDetailedAnalytics", result.IsDetailedAnalyticsAllowed.ToString());
#else
            PlayerPrefs.SetString("IsDetailedAnalytics", result.IsDetailedAnalyticsAllowed.ToString());
#endif
            return result.IsDetailedAnalyticsAllowed;
        }
        #endregion

        #region Functionaly
        protected enum RunWithHandlingErrorStatus
        {
            Successful,
            Cancelled,
            SignedOut
        }

        protected async UniTask<(RunWithHandlingErrorStatus Status, TResult Result)> RunWithHandlingError<TResult>(CancellationToken cancellationToken, Func<UniTask<TResult>> func)
        {
            if (cancellationToken.IsCancellationRequested)
                return (RunWithHandlingErrorStatus.Cancelled, default);
            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return (RunWithHandlingErrorStatus.Cancelled, default);
                    return (RunWithHandlingErrorStatus.Successful, await UniTask.Create(func));
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return (RunWithHandlingErrorStatus.Cancelled, default);
                    _logger.Log("Handled error: " + ex.Message + cancellationToken.IsCancellationRequested);
                    ErrorData error;
                    if (typeof(ErrorDataException).IsAssignableFrom(ex.GetType()))
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
        protected async UniTask<RunWithHandlingErrorStatus> RunWithHandlingError(CancellationToken cancellationToken, Func<UniTask> func)
        {
            if (cancellationToken.IsCancellationRequested)
                return RunWithHandlingErrorStatus.Cancelled;
            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return RunWithHandlingErrorStatus.Cancelled;
                    await UniTask.Create(func);
                    return RunWithHandlingErrorStatus.Successful;
                }
                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested || ex is OperationCanceledException)
                        return RunWithHandlingErrorStatus.Cancelled;
                    _logger.Log("Handled error: " + ex.Message + cancellationToken.IsCancellationRequested);
                    ErrorData error;
                    if (typeof(ErrorDataException).IsAssignableFrom(ex.GetType()))
                        error = (ex as ErrorDataException).Error;
                    else
                        error = new ErrorData("Unknown Error", ex.Message, canProceed: true, canRetry: true, mustSignOut: true);
                    var errorResultValue = await AddAndProcessContentAsync<ErrorContent, ErrorContentResult>(cancellationToken, error);
                    switch (errorResultValue.Method)
                    {
                        case ErrorHandlingMethod.Close:
                            return RunWithHandlingErrorStatus.Cancelled;
                        case ErrorHandlingMethod.TryAgain:
                            break;
                        case ErrorHandlingMethod.SignOut:
                            return RunWithHandlingErrorStatus.SignedOut;
                    }
                }
            }
        }

        protected async UniTask SendRequestAsync<TRequest>(CancellationToken cancellationToken,
                    string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, TRequest requestData, bool isBackground = false)
                                where TRequest : Data
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
                    GetMethod => Get(endpoint, authType, authData,
                                                requestData.ToDictionary().ToList().Select((el) => (el.Key, el.Value)).ToArray()),
                    PutMethod => Put(endpoint, authType, authData, requestData),
                    DeleteMethod => Delete(endpoint, authType, authData, requestData),
                    _ => Post(endpoint, authType, authData, requestData),
                };
                Func<UniTask> request = async () => await task;

                try
                {
                    if (isBackground)
                        await UniTask.Create(request);
                    else
                        await AddAndProcessContentAsync<LoadingContent, LoadingContentResult>(
                            cancellationToken, LoadingContentType.Empty, request);
                    return;
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
                    GetMethod => Get<TResponse>(endpoint, authType, authData,
                                                requestData.ToDictionary().ToList().Select((el) => (el.Key, el.Value)).ToArray()),
                    PutMethod => Put<TResponse>(endpoint, authType, authData, requestData),
                    DeleteMethod => Delete<TResponse>(endpoint, authType, authData, requestData),
                    _ => Post<TResponse>(endpoint, authType, authData, requestData),
                };
                Func<UniTask<Data>> request = async () => await task;

                try
                {
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
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, object value)[] requestDataFields)
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
            string requestName, string endpoint, string method, AuthorizationType authType, Func<string> getAuthData, bool isBackground = false, params (string key, object value)[] requestDataFields)
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
                    return;
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
            AuthController.OnSignedIn += (user) =>
            {
                _logger.Log($"OnSignedIn({JsonConvert.SerializeObject(user)})");
                OnSignedIn?.Invoke(User);
                _onSignedInCTS?.Cancel();
                _onSignedOutCTS = new();
                ShowUserContentAsync(_onSignedOutCTS.Token).Forget();
            };
            AuthController.OnSignedOut += () =>
            {
                _logger.Log($"OnSignedOut()");
                SetOpenState(true);
                OnSignedOut?.Invoke();
                _onSignedOutCTS?.Cancel();
                _onSignedInCTS = new();
                SignInAsync(_onSignedInCTS.Token).Forget();
            };
            _logger.Log("InitializeEvents()");
        }
        private protected virtual void InitializeUI()
        {
            _logger.Log("InitializeUI()");
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