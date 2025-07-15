using System;
using System.Runtime.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Auth.Core.Models;
using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Shared.Core.Contracts.Dto.User;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;
using UnityEngine;

namespace Energy8.Identity.User.Core.Services
{
    public class UserService : IUserService
    {
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
                Debug.Log("Fetching user profile");
                return await httpClient.GetAsync<UserDto>("user", ct);
            }
            catch (Exception ex)
            {
                Debug.LogError("User profile fetch failed: " + ex.Message);
                throw new UserServiceException("Failed to get user profile", ex);
            }
        }

        public async UniTask UpdateNameAsync(string name, CancellationToken ct)
        {
            try
            {
                Debug.Log("Updating user name: " + name);
                await httpClient.PutAsync("user/name", new { Name = name }, ct);
            }
            catch (Exception ex)
            {
                Debug.LogError("User name update failed: " + ex.Message);
                throw new UserServiceException("Failed to update user name", ex);
            }
        }

        public async UniTask<string> RequestEmailChangeAsync(string newEmail, CancellationToken ct)
        {
            try
            {
                Debug.Log("Email change requested: " + newEmail);
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
                Debug.LogError("Email change request failed: " + ex.Message);
                throw new UserServiceException("Failed to request email change", ex);
            }
        }

        public async UniTask ConfirmEmailChangeAsync(string token, string code, CancellationToken ct)
        {
            try
            {
                Debug.Log("Confirming email change");
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
                Debug.LogError("Email change confirmation failed: " + ex.Message);
                throw new UserServiceException("Failed to confirm email change", ex);
            }
        }

        public async UniTask<string> RequestDeleteAccountAsync(CancellationToken ct)
        {
            try
            {
                Debug.Log("Account deletion requested");
                var response = await httpClient.DeleteAsync<EmailVerificationTokenDto>(
                    "user",
                    ct);
                return response.Token;
            }
            catch (Exception ex)
            {
                Debug.LogError("Account deletion request failed: " + ex.Message);
                throw new UserServiceException("Failed to request account deletion", ex);
            }
        }

        public async UniTask ConfirmDeleteAccountAsync(string token, string code, CancellationToken ct)
        {
            try
            {
                Debug.Log("Confirming account deletion");
                await httpClient.DeleteAsync(
                    "user/confirm",
                    new { Token = token, Code = code },
                    ct);
            }
            catch (Exception ex)
            {
                Debug.LogError("Account deletion confirmation failed: " + ex.Message);
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
                Debug.Log($"Unlinking provider: {provider}");
                await httpClient.PostAsync<object>(
                    "User/UnlinkProvider",
                    new { Provider = provider },
                    ct);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to unlink provider: {ex.Message}");
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