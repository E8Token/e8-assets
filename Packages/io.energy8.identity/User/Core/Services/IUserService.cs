using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Auth.Core.Models;
using Energy8.Identity.Shared.Core.Contracts.Dto.User;

namespace Energy8.Identity.User.Core.Services
{
    public interface IUserService
    {
        UniTask<UserDto> GetUserAsync(CancellationToken ct);
        UniTask UpdateNameAsync(string name, CancellationToken ct);

        // Email management
        UniTask<string> RequestEmailChangeAsync(string newEmail, CancellationToken ct);
        UniTask ConfirmEmailChangeAsync(string token, string code, CancellationToken ct);

        // Account management
        UniTask<string> RequestDeleteAccountAsync(CancellationToken ct);
        UniTask ConfirmDeleteAccountAsync(string token, string code, CancellationToken ct);

        // Provider management
        UniTask<bool> IsProviderLinkedAsync(AuthProviderType provider, CancellationToken ct);
        UniTask UnlinkProviderAsync(AuthProviderType provider, CancellationToken ct);
    }
}