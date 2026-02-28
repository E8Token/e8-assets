using System;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#endif

namespace Energy8.Identity.UI.Runtime.Services
{
    public interface IIdentityService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        bool HasTelegramAutoAuthData { get; }
        event Action OnSignedOut;

        UniTask Initialize(CancellationToken ct);
        void EnableTokenLogging(bool enabled);
        UniTask<object> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask SignOut(CancellationToken ct);
        UniTask StartEmailFlow(string email, CancellationToken ct);
        UniTask<object> ConfirmEmailCode(string code, CancellationToken ct);
        UniTask<object> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<object> SignInWithTelegramAsync(bool linkProvider, CancellationToken ct);
    }
}
