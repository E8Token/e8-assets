#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Auth.Core.Providers;
using Newtonsoft.Json;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;
using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using UnityEngine;
using Energy8.Identity.Auth.Core.Models;
using Energy8.Identity.Auth.WebGL.Plugins;
using Energy8.Identity.Configuration.Core;

namespace Energy8.Identity.Auth.Runtime.Providers
{
    public class WebGLAuthProvider : IAuthProvider
    {
        private readonly FirebaseWebGLAuthPlugin plugin;
        private UniTaskCompletionSource<TelegramUserDto> telegramAutoAuthTcs;
        
        // Поле для хранения TelegramUserDto после автоматической аутентификации
        private TelegramUserDto autoAuthTelegramUser = null;
        private bool autoAuthInitialized = false;

        public bool IsSignedIn => CurrentUser != null;
        public FirebaseUser CurrentUser { get; private set; }

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;
        
        // Свойство для проверки процесса автоаутентификации
        public bool IsAutoAuthenticating { get; private set; }
        
        // Реализация свойства из интерфейса IAuthProvider
        public bool HasTelegramAutoAuthData => autoAuthTelegramUser != null;

        public WebGLAuthProvider()
        {
            plugin = FirebaseWebGLAuthPlugin.Instance;
            plugin.OnSignIn += HandleSignIn;
            plugin.OnSignOut += HandleSignOut;
            plugin.OnError += HandleError;
            plugin.OnTelegramAuth += HandleTelegramAuth;
            // Добавляем обработчик нового события
            plugin.OnTelegramAutoAuthComplete += HandleTelegramAutoAuthComplete;
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            Debug.Log("Initializing WebGLAuthProvider");
            
            // Создаем экземпляр TaskCompletionSource перед инициализацией плагина
            telegramAutoAuthTcs = new UniTaskCompletionSource<TelegramUserDto>();
            
            // Initialize the Firebase plugin
            await plugin.Initialize(IdentityConfiguration.AuthConfig);
            
            // Проверяем наличие автоаутентификации через Telegram
            try
            {
                IsAutoAuthenticating = true;
                Debug.Log("Waiting for auto-authentication with Telegram...");
                
                // Ждем данные автоаутентификации или таймаут (увеличено до 5 секунд)
                UniTask<TelegramUserDto> telegramTask = telegramAutoAuthTcs.Task;
                
                var (hasResult, _) = await UniTask.WhenAny(
                    telegramTask,
                    UniTask.Delay(5000, cancellationToken: ct)
                );
                
                if (hasResult)
                {
                    // Получаем данные пользователя из завершенной задачи
                    try 
                    {
                        autoAuthTelegramUser = await telegramTask;
                        Debug.Log($"Telegram auto-authentication completed successfully for user: {autoAuthTelegramUser.FirstName} {autoAuthTelegramUser.LastName}");
                    }
                    catch (Exception ex) 
                    {
                        Debug.LogError($"Error retrieving Telegram user data: {ex.Message}");
                        autoAuthTelegramUser = null;
                    }
                }
                else
                {
                    Debug.LogWarning("Telegram auto-authentication timed out");
                    autoAuthTelegramUser = null;
                }
                
                IsAutoAuthenticating = false;
                autoAuthInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during Telegram auto-authentication: {ex.Message}");
                IsAutoAuthenticating = false;
                autoAuthInitialized = true;
            }
        }

        // Добавляем метод для обработки события завершения автоаутентификации
        private void HandleTelegramAutoAuthComplete()
        {
            Debug.Log("Telegram auto-auth flow completed signal received");
            
            // Если мы еще не завершили автоаутентификацию и telegramAutoAuthTcs не завершен,
            // попробуем завершить его с null, чтобы хотя бы разблокировать ожидание
            if (IsAutoAuthenticating && !autoAuthInitialized && telegramAutoAuthTcs != null)
            {
                if (autoAuthTelegramUser != null)
                {
                    telegramAutoAuthTcs.TrySetResult(autoAuthTelegramUser);
                    Debug.Log("Auto-auth completion signal completed the waiting task with actual data");
                }
                else
                {
                    Debug.LogWarning("Auto-auth completion signal tried to complete the task, but no user data was available");
                }
            }
        }

        public async UniTask<string> GetToken(bool forceRefresh, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleToken(string token) => tcs.TrySetResult(token);
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            plugin.OnTokenReceived += HandleToken;
            plugin.OnError += HandleError;

            try
            {
                plugin.GetToken(forceRefresh);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            finally
            {
                plugin.OnTokenReceived -= HandleToken;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Token sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithToken(token);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Google sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithGoogleProvider(linkProvider);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Apple sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithAppleProvider(linkProvider);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct)
        {
            // Если у нас уже есть данные пользователя Telegram из автоматической аутентификации,
            // возвращаем их без необходимости вызывать Telegram снова
            if (autoAuthTelegramUser != null)
            {
                Debug.Log($"Using captured Telegram auto-auth data for user: {autoAuthTelegramUser.FirstName} {autoAuthTelegramUser.LastName}");
                
                // Очищаем данные после использования
                var user = autoAuthTelegramUser;
                autoAuthTelegramUser = null;
                
                return user;
            }
            
            // Стандартная логика вызова Telegram аутентификации, если у нас нет данных
            var tcs = new UniTaskCompletionSource<TelegramUserDto>();

            void HandleSuccess(string telegramUserJson)
            {
                try
                {
                    var telegramUser = JsonConvert.DeserializeObject<TelegramUserDto>(telegramUserJson);
                    Debug.Log($"[TelegramAuth] HandleSuccess JsonConvert SUCCESS - ID={telegramUser.Id}, Name={telegramUser.FirstName} {telegramUser.LastName}, Username={telegramUser.Username}, Hash={telegramUser.Hash}, PhotoUrl={telegramUser.PhotoUrl}");
                    tcs.TrySetResult(telegramUser);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TelegramAuth] HandleSuccess JsonConvert FAILED: {ex.Message}");
                    tcs.TrySetException(new Exception($"User not parsed: {ex.Message}"));
                }
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Telegram sign in failed: {error}"));

            plugin.OnTelegramAuth += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithTelegramProvider();
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnTelegramAuth -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public void SignOut()
        {
            plugin.SignOutUser();
        }

        private void HandleSignIn(string userJson)
        {
            CurrentUser = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
            OnSignedIn?.Invoke(CurrentUser);
        }

        private void HandleSignOut()
        {
            CurrentUser = null;
            OnSignedOut?.Invoke();
        }

        private void HandleError(string error)
        {
            Debug.LogError($"Firebase error: {error}");
            telegramAutoAuthTcs?.TrySetException(new Exception(error));
        }
        
        private void HandleTelegramAuth(string telegramUserJson)
        {
            Debug.Log($"[TelegramAuth] Received data from JavaScript: {telegramUserJson}");
            Debug.Log($"[TelegramAuth] Data length: {telegramUserJson?.Length ?? 0} characters");
            
            // Check if Hash and PhotoUrl are present in raw data
            bool hasHashInRaw = !string.IsNullOrEmpty(telegramUserJson) && telegramUserJson.Contains("hash=");
            bool hasPhotoUrlInRaw = !string.IsNullOrEmpty(telegramUserJson) && telegramUserJson.Contains("photo_url=");
            Debug.Log($"[TelegramAuth] Raw data contains hash: {hasHashInRaw}, photo_url: {hasPhotoUrlInRaw}");
            
            // Initialize the completion source if needed
            if (telegramAutoAuthTcs == null)
            {
                telegramAutoAuthTcs = new UniTaskCompletionSource<TelegramUserDto>();
            }
            
            try 
            {
                TelegramUserDto telegramUser = null;
                
                // Debug output: Print the raw data
                Debug.Log($"[TelegramAuth] Raw data: {telegramUserJson}");
                Debug.Log($"[TelegramAuth] Data length: {telegramUserJson?.Length}, Contains hash: {telegramUserJson?.Contains("hash")}, Contains photo_url: {telegramUserJson?.Contains("photo_url")}");
                
                // Parse the query string format from Telegram
                if (telegramUserJson.Contains("id=") && telegramUserJson.Contains("hash="))
                {
                    // This is a query string format from Telegram authentication
                    Debug.Log("Detected query string format, parsing parameters...");
                    
                    // Parse the query string
                    var parameters = new System.Collections.Generic.Dictionary<string, string>();
                    string[] parts = telegramUserJson.Split('&');
                    
                    foreach (var part in parts)
                    {
                        string[] keyValue = part.Split('=');
                        if (keyValue.Length == 2)
                        {
                            string key = keyValue[0];
                            string value = Uri.UnescapeDataString(keyValue[1]);
                            parameters[key] = value;
                        }
                    }
                    
                    // Check if this is WebApp format with 'user' field containing JSON
                    if (parameters.ContainsKey("user") && parameters["user"].StartsWith("{"))
                    {
                        Debug.Log("Detected WebApp format with user JSON, parsing user data...");
                        
                        try
                        {
                            var userJson = parameters["user"];
                            var jObject = Newtonsoft.Json.Linq.JObject.Parse(userJson);
                            
                            // Extract required fields from user JSON
                            if (jObject.TryGetValue("id", out var idToken) &&
                                jObject.TryGetValue("first_name", out var firstNameToken))
                            {
                                long webAppId = idToken.ToObject<long>();
                                string webAppFirstName = firstNameToken.ToString();
                                
                                // Get optional fields from user JSON
                                string webAppLastName = jObject["last_name"]?.ToString() ?? "";
                                string webAppUsername = jObject["username"]?.ToString() ?? "";
                                string webAppPhotoUrl = jObject["photo_url"]?.ToString()?.Trim('`', ' ') ?? "";
                                string webAppLanguageCode = jObject["language_code"]?.ToString() ?? "";
                                bool webAppAllowsWriteToPm = jObject["allows_write_to_pm"]?.ToObject<bool>() ?? false;
                                
                                // Get fields from main parameters
                                parameters.TryGetValue("hash", out string webAppHash);
                                parameters.TryGetValue("query_id", out string webAppQueryId);
                                
                                // If hash is not in main parameters, try to get it from user JSON
                                if (string.IsNullOrEmpty(webAppHash))
                                {
                                    webAppHash = jObject["hash"]?.ToString() ?? "";
                                }
                                
                                Debug.Log($"[TelegramAuth] WebApp Hash: '{webAppHash}', PhotoUrl: '{webAppPhotoUrl}'");
                                
                                // Validate required fields for server
                                if (string.IsNullOrEmpty(webAppHash))
                                {
                                    Debug.LogError("[TelegramAuth] Hash is required but missing from Telegram data");
                                    telegramAutoAuthTcs.TrySetException(new Exception("Hash field is required for Telegram authentication"));
                                    return;
                                }
                                
                                // Parse auth_date
                                long authDate = 0;
                                if (parameters.TryGetValue("auth_date", out string authDateStr))
                                {
                                    long.TryParse(authDateStr, out authDate);
                                }
                                
                                // Validate required fields for server
                                if (string.IsNullOrEmpty(webAppHash))
                                {
                                    Debug.LogError("[TelegramAuth] Hash is required but missing from Telegram WebApp data");
                                    telegramAutoAuthTcs.TrySetException(new Exception("Hash field is required for Telegram authentication"));
                                    return;
                                }
                                
                                if (string.IsNullOrEmpty(webAppPhotoUrl))
                                {
                                    Debug.LogError("[TelegramAuth] PhotoUrl is required but missing from Telegram WebApp data");
                                    telegramAutoAuthTcs.TrySetException(new Exception("PhotoUrl field is required for Telegram authentication"));
                                    return;
                                }
                                
                                // Create the TelegramUserDto
                                telegramUser = new TelegramUserDto(
                                    hash: webAppHash,
                                    id: webAppId,
                                    firstName: webAppFirstName,
                                    lastName: webAppLastName,
                                    username: webAppUsername,
                                    photoUrl: webAppPhotoUrl,
                                    authDate: authDate,
                                    languageCode: webAppLanguageCode,
                                    allowsWriteToPm: webAppAllowsWriteToPm,
                                    queryId: webAppQueryId
                                );
                                
                                Debug.Log($"Successfully created Telegram user from WebApp format: ID={telegramUser.Id}, Name={telegramUser.FirstName} {telegramUser.LastName}, Username={telegramUser.Username}");
                                telegramAutoAuthTcs.TrySetResult(telegramUser);
                                return;
                            }
                            else
                            {
                                Debug.LogError("Missing required fields (id, first_name) in user JSON");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error parsing WebApp user JSON: {ex.Message}");
                        }
                    }
                    
                    // Extract the required fields
                    if (parameters.TryGetValue("id", out string idStr) && 
                        parameters.TryGetValue("first_name", out string firstName) &&
                        parameters.TryGetValue("hash", out string hash))
                    {
                        // Get optional fields
                        parameters.TryGetValue("last_name", out string lastName);
                        parameters.TryGetValue("username", out string username);
                        parameters.TryGetValue("photo_url", out string photoUrl);
                        parameters.TryGetValue("auth_date", out string authDateStr);
                        parameters.TryGetValue("language_code", out string languageCode);
                        parameters.TryGetValue("query_id", out string queryId);
                        
                        // Parse the allows_write_to_pm parameter if it exists
                        bool allowsWriteToPm = false;
                        if (parameters.TryGetValue("allows_write_to_pm", out string allowsWriteStr))
                        {
                            if (bool.TryParse(allowsWriteStr, out bool allowsWrite))
                            {
                                allowsWriteToPm = allowsWrite;
                            }
                            else if (allowsWriteStr == "1" || allowsWriteStr.ToLower() == "true")
                            {
                                allowsWriteToPm = true;
                            }
                            
                            // Parsed allows_write_to_pm silently
                        }
                        
                        // Parse id and auth_date
                        if (long.TryParse(idStr, out long id))
                        {
                            long authDate = 0;
                            if (!string.IsNullOrEmpty(authDateStr))
                            {
                                long.TryParse(authDateStr, out authDate);
                            }
                            
                            Debug.Log($"[TelegramAuth] Standard Hash: '{hash}', PhotoUrl: '{photoUrl?.Trim('`', ' ')}'");
                            
                            // Validate required fields for server
                            if (string.IsNullOrEmpty(hash))
                            {
                                Debug.LogError("[TelegramAuth] Hash is required but missing from Telegram data");
                                telegramAutoAuthTcs.TrySetException(new Exception("Hash field is required for Telegram authentication"));
                                return;
                            }
                            
                            // Clean and validate PhotoUrl
                            string cleanPhotoUrl = photoUrl?.Trim('`', ' ') ?? "";
                            if (string.IsNullOrEmpty(cleanPhotoUrl))
                            {
                                Debug.LogError("[TelegramAuth] PhotoUrl is required but missing from Telegram data");
                                telegramAutoAuthTcs.TrySetException(new Exception("PhotoUrl field is required for Telegram authentication"));
                                return;
                            }
                            
                            // Create the TelegramUserDto
                            telegramUser = new TelegramUserDto(
                                hash: hash,
                                id: id,
                                firstName: firstName ?? "",
                                lastName: lastName ?? "",
                                username: username ?? "",
                                photoUrl: cleanPhotoUrl,
                                authDate: authDate,
                                languageCode: languageCode,
                                allowsWriteToPm: allowsWriteToPm,
                                queryId: queryId
                            );
                            
                            // Log final DTO values
                            Debug.Log($"[TelegramAuth] Created DTO - Hash: '{telegramUser.Hash}', PhotoUrl: '{telegramUser.PhotoUrl}'");
                            Debug.Log($"[TelegramAuth] DTO Hash IsNullOrEmpty: {string.IsNullOrEmpty(telegramUser.Hash)}, PhotoUrl IsNullOrEmpty: {string.IsNullOrEmpty(telegramUser.PhotoUrl)}");
                            
                            Debug.Log($"Successfully created Telegram user from query params: ID={telegramUser.Id}, Name={telegramUser.FirstName} {telegramUser.LastName}, Username={telegramUser.Username}");
                            telegramAutoAuthTcs.TrySetResult(telegramUser);
                            return;
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse user ID: {idStr}");
                        }
                    }
                    else
                    {
                        var availableParams = string.Join(", ", parameters.Keys);
                        var debugInfo = $"Missing required parameters for Telegram authentication. Available: {availableParams}";
                        
                        // Add user field content for debugging if present
                        if (parameters.ContainsKey("user"))
                        {
                            debugInfo += $"\nUser field content: {parameters["user"]}";
                        }
                        
                        Debug.LogError(debugInfo);
                    }
                }
                // Try parsing as JSON directly
                else if (telegramUserJson.StartsWith("{") && telegramUserJson.EndsWith("}"))
                {
                    Debug.Log("Attempting to parse as JSON object");
                    
                    try
                    {
                        // Try direct JSON deserialization using Newtonsoft.Json (supports JsonProperty attributes)
                        try
                        {
                            telegramUser = JsonConvert.DeserializeObject<TelegramUserDto>(telegramUserJson);
                            if (telegramUser != null && telegramUser.Id > 0)
                            {
                                Debug.Log($"[TelegramAuth] JsonConvert SUCCESS - ID={telegramUser.Id}, Name={telegramUser.FirstName} {telegramUser.LastName}, Username={telegramUser.Username}, Hash={telegramUser.Hash}, PhotoUrl={telegramUser.PhotoUrl}");
                                telegramAutoAuthTcs.TrySetResult(telegramUser);
                                return;
                            }
                            else
                            {
                                Debug.Log($"[TelegramAuth] JsonConvert returned null or invalid user (ID={telegramUser?.Id}). Falling back to manual parsing.");
                            }
                        }
                        catch (Exception jsonEx)
                        {
                            Debug.Log($"[TelegramAuth] JsonConvert FAILED: {jsonEx.Message}. Falling back to manual parsing.");
                        }
                        
                        // Fallback: Try manual parsing using JObject
                        {
                            // Try manual parsing using JObject
                            var jObject = Newtonsoft.Json.Linq.JObject.Parse(telegramUserJson);
                            
                            // Extract required fields
                            if (jObject.TryGetValue("id", out var idToken) &&
                                jObject.TryGetValue("first_name", out var firstNameToken))
                            {
                                long id = idToken.ToObject<long>();
                                string firstName = firstNameToken.ToString();
                                
                                // Get optional fields
                                string lastName = jObject["last_name"]?.ToString() ?? "";
                                string username = jObject["username"]?.ToString() ?? "";
                                string photoUrl = jObject["photo_url"]?.ToString()?.Trim('`', ' ') ?? "";
                                string hash = jObject["hash"]?.ToString() ?? "";
                                
                                // Try to get auth_date
                                long authDate = 0;
                                if (jObject.TryGetValue("auth_date", out var authDateToken))
                                {
                                    authDate = authDateToken.ToObject<long>();
                                }
                                
                                Debug.Log($"[TelegramAuth] JSON Hash: '{hash}', PhotoUrl: '{photoUrl}'");
                                
                                // Validate required fields for server
                                if (string.IsNullOrEmpty(hash))
                                {
                                    Debug.LogError("[TelegramAuth] Hash is required but missing from Telegram JSON data");
                                    telegramAutoAuthTcs.TrySetException(new Exception("Hash field is required for Telegram authentication"));
                                    return;
                                }
                                
                                // Create the TelegramUserDto
                                telegramUser = new TelegramUserDto(
                                    hash: hash,
                                    id: id,
                                    firstName: firstName,
                                    lastName: lastName,
                                    username: username,
                                    photoUrl: photoUrl,
                                    authDate: authDate,
                                    languageCode: jObject["language_code"]?.ToString() ?? "",
                                    allowsWriteToPm: jObject["allows_write_to_pm"]?.ToObject<bool>() ?? false
                                );
                                
                                // Log final DTO values from JSON
                                Debug.Log($"[TelegramAuth] Created DTO from JSON - Hash: '{telegramUser.Hash}', PhotoUrl: '{telegramUser.PhotoUrl}'");
                                Debug.Log($"[TelegramAuth] JSON DTO Hash IsNullOrEmpty: {string.IsNullOrEmpty(telegramUser.Hash)}, PhotoUrl IsNullOrEmpty: {string.IsNullOrEmpty(telegramUser.PhotoUrl)}");
                                
                                Debug.Log($"Successfully created Telegram user from JObject: ID={telegramUser.Id}, Name={telegramUser.FirstName} {telegramUser.LastName}, Username={telegramUser.Username}");
                                telegramAutoAuthTcs.TrySetResult(telegramUser);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing Telegram JSON data: {ex.Message}");
                    }
                }
                
                // If we couldn't parse the data with any method
                Debug.LogError($"Failed to parse Telegram user data with any method. Raw data: {telegramUserJson}");
                telegramAutoAuthTcs.TrySetException(new Exception("Could not parse Telegram authentication data"));
            }
            catch (Exception ex)
            {
                var error = $"Error processing Telegram data: {ex.Message}";
                Debug.LogError(error);
                telegramAutoAuthTcs.TrySetException(new Exception(error));
            }
        }
    }
}
#endif