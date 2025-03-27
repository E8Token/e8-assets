using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Contracts.Dto.Auth;



#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.Core.Auth.Providers
{
    public interface IAuthProvider
    {
        bool IsSignedIn { get; }
        FirebaseUser CurrentUser { get; }

        UniTask Initialize(CancellationToken ct);
        UniTask<string> GetToken(bool forceRefresh, CancellationToken ct);
        UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct);
        UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct);
        UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct);
        UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct);
        void SignOut();

        event Action<FirebaseUser> OnSignedIn;
        event Action OnSignedOut;
    }
}