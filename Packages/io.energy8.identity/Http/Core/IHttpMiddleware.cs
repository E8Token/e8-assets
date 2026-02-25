using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Core
{
    /// <summary>
    /// Интерфейс для HTTP middleware
    /// Middleware выполняется в цепочке для обработки запросов и ответов
    /// </summary>
    public interface IHttpMiddleware
    {
        /// <summary>
        /// Имя middleware для логирования и отладки
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Обрабатывает HTTP запрос
        /// </summary>
        /// <param name="request">HTTP запрос для обработки</param>
        /// <param name="next">Следующий middleware в цепочке</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>HTTP ответ</returns>
        UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct);
    }

    /// <summary>
    /// Делегат для передачи управления следующему middleware
    /// </summary>
    /// <param name="request">HTTP запрос</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>HTTP ответ</returns>
    public delegate UniTask<HttpResponse> HttpMiddlewareDelegate(HttpRequest request, CancellationToken ct);
}
