using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;
using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Newtonsoft.Json;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#endif

namespace Energy8.Identity.Auth.Core.Providers
{
    public interface IAuthProvider
    {
        bool IsSignedIn { get; }
        bool HasTelegramAutoAuthData { get; }

        UniTask Initialize(CancellationToken ct);
        UniTask<string> GetToken(bool forceRefresh, CancellationToken ct);
        UniTask<object> SignInWithToken(string token, CancellationToken ct);
        UniTask<object> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask<object> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct);
        void SignOut();

        event Action<object> OnSignedIn;
        event Action OnSignedOut;
    }
}