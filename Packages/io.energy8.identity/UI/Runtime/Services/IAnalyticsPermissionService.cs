using Cysharp.Threading.Tasks;
using System.Threading;

namespace Energy8.Identity.UI.Runtime.Services
{
    /// <summary>
    /// Сервис для управления разрешениями на аналитику
    /// </summary>
    public interface IAnalyticsPermissionService
    {
        /// <summary>
        /// Проверяет, было ли запрошено разрешение на аналитику
        /// </summary>
        bool IsAnalyticsPermissionRequested { get; }
        
        /// <summary>
        /// Получает текущее разрешение на аналитику
        /// </summary>
        bool HasAnalyticsPermission { get; }
        
        /// <summary>
        /// Проверяет, нужно ли показать запрос разрешения на аналитику
        /// </summary>
        bool ShouldShowAnalyticsPermissionRequest { get; }
        
        /// <summary>
        /// Запрашивает разрешение на аналитику у пользователя
        /// </summary>
        /// <param name="ct">Токен отмены</param>
        /// <returns>True если пользователь дал разрешение, false если отказал</returns>
        UniTask<bool> RequestAnalyticsPermissionAsync(CancellationToken ct = default);
        
        /// <summary>
        /// Сохраняет разрешение на аналитику
        /// </summary>
        /// <param name="granted">Разрешение дано или нет</param>
        void SaveAnalyticsPermission(bool granted);
    }
}
