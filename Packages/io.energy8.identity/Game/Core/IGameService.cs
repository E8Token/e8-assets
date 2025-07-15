using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;

namespace Energy8.Identity.Game.Core.Services
{
    public interface IGameService
    {
        UniTask<GameUserDto> GetUserAsync(CancellationToken ct);
        UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct);
    }
}