using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Runtime.Flows
{
    /// <summary>
    /// Интерфейс для управления авторизационными потоками
    /// </summary>
    public interface IAuthFlowManager
    {
        /// <summary>
        /// Запуск потока авторизации
        /// </summary>
        UniTask StartAuthFlowAsync(CancellationToken ct);
    }
}
