using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Shared.Core.Error;
using Energy8.Identity.Shared.Core.Exceptions;

namespace Energy8.Identity.UI.Core
{
    /// <summary>
    /// Интерфейс для централизованной обработки ошибок
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Показывает ошибку пользователю и возвращает выбранное действие
        /// </summary>
        UniTask<ErrorHandlingMethod> ShowErrorAsync(Energy8Exception e8Exception, CancellationToken ct);
    }
}
