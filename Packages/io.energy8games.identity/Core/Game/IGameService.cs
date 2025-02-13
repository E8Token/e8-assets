using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Contracts.Dto.Games;

namespace Energy8.Identity.Core.Game.Services
{
    public interface IGameService
    {
        UniTask<GameUserDto> GetUserAsync(CancellationToken ct);
        UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct);
    }
}