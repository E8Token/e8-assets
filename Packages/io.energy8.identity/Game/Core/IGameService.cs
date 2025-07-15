using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Contracts.Dto.Games;

namespace Energy8.Identity.Game.Core.Services
{
    /// <summary>
    /// Interface for game-related operations
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Gets the current game user data
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Game user data</returns>
        UniTask<GameUserDto> GetUserAsync(CancellationToken ct);
        
        /// <summary>
        /// Creates a new game session
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Game session data</returns>
        UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct);
    }
}