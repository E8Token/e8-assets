using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;
using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Newtonsoft.Json;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.Auth.Core.Providers
{
    public interface IAuthProvider
    {
        bool IsSignedIn { get; }
        FirebaseUser CurrentUser { get; }
        bool HasTelegramAutoAuthData { get; }

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