using System;
using System.Threading;
using Cysharp.Threading.Tasks;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.Services
{
    public interface IIdentityService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        FirebaseUser CurrentUser { get; }
        bool HasTelegramAutoAuthData { get; }

        event Action<FirebaseUser> OnSignedIn;
        event Action OnSignedOut;

        UniTask Initialize(CancellationToken ct);
        UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask SignOut(CancellationToken ct);
        UniTask StartEmailFlow(string email, CancellationToken ct);
        UniTask<AuthResult> ConfirmEmailCode(string code, CancellationToken ct);
        UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<AuthResult> SignInWithTelegramAsync(bool linkProvider, CancellationToken ct);
    }
}
