using System;
using System.Runtime.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Auth.Models;
using Energy8.Identity.Core.Auth.Providers;
using Energy8.Identity.Core.Http;
using Energy8.Contracts.Dto.User;
using Energy8.Contracts.Dto.Auth;

namespace Energy8.Identity.Core.User.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> logger = new Logger<UserService>();
        private readonly IHttpClient httpClient;
        private readonly IAuthProvider authProvider;

        public UserService(IHttpClient httpClient, IAuthProvider authProvider)
        {
            this.httpClient = httpClient;
            this.authProvider = authProvider;
        }

        public async UniTask<UserDto> GetUserAsync(CancellationToken ct)
        {
            try
            {
                logger.LogDebug("Fetching user profile");
                return await httpClient.GetAsync<UserDto>("user", ct);
            }
            catch (Exception ex)
            {
                logger.LogError("User profile fetch failed", ex.Message);
                throw new UserServiceException("Failed to get user profile", ex);
            }
        }

        public async UniTask UpdateNameAsync(string name, CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Updating user name", name);
                await httpClient.PutAsync("user/name", new { Name = name }, ct);
            }
            catch (Exception ex)
            {
                logger.LogError("User name update failed", ex.Message);
                throw new UserServiceException("Failed to update user name", ex);
            }
        }

        public async UniTask<string> RequestEmailChangeAsync(string newEmail, CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Email change requested", newEmail);
                var response = await httpClient.PutAsync<EmailVerificationTokenDto>(
                    "user/email",
                    new EmailChangeDto()
                    {
                        NewEmail = newEmail
                    },
                    ct);
                return response.Token;
            }
            catch (Exception ex)
            {
                logger.LogError("Email change request failed", ex.Message);
                throw new UserServiceException("Failed to request email change", ex);
            }
        }

        public async UniTask ConfirmEmailChangeAsync(string token, string code, CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Confirming email change");
                await httpClient.PutAsync(
                    "user/email/confirm",
                    new EmailConfirmDto()
                    {
                        Token = token,
                        Code = code
                    },
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError("Email change confirmation failed", ex.Message);
                throw new UserServiceException("Failed to confirm email change", ex);
            }
        }

        public async UniTask<string> RequestDeleteAccountAsync(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Account deletion requested");
                var response = await httpClient.DeleteAsync<EmailVerificationTokenDto>(
                    "user",
                    ct);
                return response.Token;
            }
            catch (Exception ex)
            {
                logger.LogError("Account deletion request failed", ex.Message);
                throw new UserServiceException("Failed to request account deletion", ex);
            }
        }

        public async UniTask ConfirmDeleteAccountAsync(string token, string code, CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Confirming account deletion");
                await httpClient.DeleteAsync(
                    "user/confirm",
                    new { Token = token, Code = code },
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError("Account deletion confirmation failed", ex.Message);
                throw new UserServiceException("Failed to confirm account deletion", ex);
            }
        }

        public async UniTask<bool> IsProviderLinkedAsync(AuthProviderType provider, CancellationToken ct)
        {
            var user = await GetUserAsync(ct);
            return user.AuthProviders.Contains(provider.ToString());
        }

        public async UniTask UnlinkProviderAsync(AuthProviderType provider, CancellationToken ct)
        {
            try
            {
                logger.LogInfo($"Unlinking provider: {provider}");
                await httpClient.PostAsync<object>(
                    "User/UnlinkProvider",
                    new { Provider = provider },
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to unlink provider: {ex.Message}");
                throw new UserServiceException("Failed to unlink provider", ex);
            }
        }

        // Other methods implementation...
    }

    internal class EmailChangeResponse
    {
        public string Token { get; internal set; }
    }

    [Serializable]
    internal class UserServiceException : Exception
    {
        public UserServiceException()
        {
        }

        public UserServiceException(string message) : base(message)
        {
        }

        public UserServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}