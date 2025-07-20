using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Runtime.Flows
{
    /// <summary>
    /// Интерфейс для управления пользовательскими потоками
    /// </summary>
    public interface IUserFlowManager
    {
        /// <summary>
        /// Запуск пользовательского потока
        /// </summary>
        UniTask StartUserFlowAsync(CancellationToken ct);
        
        /// <summary>
        /// Показ настроек пользователя
        /// </summary>
        UniTask ShowSettingsAsync(CancellationToken ct);
        
        /// <summary>
        /// Изменение имени пользователя
        /// </summary>
        UniTask ShowChangeNameAsync(CancellationToken ct);
        
        /// <summary>
        /// Изменение email пользователя
        /// </summary>
        UniTask ShowChangeEmailAsync(CancellationToken ct);
        
        /// <summary>
        /// Удаление аккаунта пользователя
        /// </summary>
        UniTask ShowDeleteAccountAsync(CancellationToken ct);
    }
}
