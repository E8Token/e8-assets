using Cysharp.Threading.Tasks;
using System.Threading;
using System;


#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif


namespace Energy8.Identity.Runtime.Services
{
    public interface IIdentityService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        FirebaseUser CurrentUser { get; }
        //UserDto User { get; }

        UniTask Initialize(CancellationToken ct);
        UniTask StartEmailFlow(string email, CancellationToken ct);
        UniTask<AuthResult> ConfirmEmailCode(string code, CancellationToken ct);
        UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<AuthResult> SignInWithTelegramAsync(bool linkProvider, CancellationToken ct);
        UniTask SignOut(CancellationToken ct);

        event Action<FirebaseUser> OnSignedIn;
        event Action OnSignedOut;
    }
}