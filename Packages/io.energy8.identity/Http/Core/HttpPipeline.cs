using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Http.Core.Models;

namespace Energy8.Identity.Http.Core
{
    /// <summary>
    /// Pipeline для выполнения HTTP middleware в цепочке
    /// Обеспечивает гибкую архитектуру обработки запросов через middleware
    /// </summary>
    public class HttpPipeline
    {
        private readonly List<IHttpMiddleware> middlewares = new();
        private readonly object lockObj = new();
        
        /// <summary>
        /// Добавляет middleware в конец цепочки
        /// </summary>
        /// <param name="middleware">Middleware для добавления</param>
        /// <returns>Текущий pipeline для цепочки вызовов</returns>
        public HttpPipeline Use(IHttpMiddleware middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
            
            lock (lockObj)
            {
                // Проверяем, что middleware с таким именём не существует
                if (middlewares.Any(m => m.Name == middleware.Name))
                {
                    throw new InvalidOperationException($"Middleware with name '{middleware.Name}' already exists in pipeline");
                }
                
                middlewares.Add(middleware);
            }
            
            return this;
        }

        /// <summary>
        /// Добавляет middleware в начало цепочки
        /// </summary>
        /// <param name="middleware">Middleware для добавления</param>
        /// <returns>Текущий pipeline для цепочки вызовов</returns>
        public HttpPipeline UseFirst(IHttpMiddleware middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
            
            lock (lockObj)
            {
                if (middlewares.Any(m => m.Name == middleware.Name))
                {
                    throw new InvalidOperationException($"Middleware with name '{middleware.Name}' already exists in pipeline");
                }
                
                middlewares.Insert(0, middleware);
            }
            
            return this;
        }

        /// <summary>
        /// Удаляет middleware по имени
        /// </summary>
        /// <param name="middlewareName">Имя middleware для удаления</param>
        /// <returns>Текущий pipeline для цепочки вызовов</returns>
        public HttpPipeline Remove(string middlewareName)
        {
            lock (lockObj)
            {
                var middleware = middlewares.FirstOrDefault(m => m.Name == middlewareName);
                if (middleware != null)
                {
                    middlewares.Remove(middleware);
                }
            }
            
            return this;
        }

        /// <summary>
        /// Очищает все middleware из pipeline
        /// </summary>
        /// <returns>Текущий pipeline для цепочки вызовов</returns>
        public HttpPipeline Clear()
        {
            lock (lockObj)
            {
                middlewares.Clear();
            }
            
            return this;
        }

        /// <summary>
        /// Выполняет HTTP запрос через весь pipeline middleware
        /// </summary>
        /// <param name="request">HTTP запрос для выполнения</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>HTTP ответ после прохождения через все middleware</returns>
        public async UniTask<HttpResponse> ExecuteAsync(HttpRequest request, CancellationToken ct)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            
            // Копируем список middleware для thread safety
            IHttpMiddleware[] middlewareArray;
            lock (lockObj)
            {
                middlewareArray = middlewares.ToArray();
            }
            
            // Создаём финальный handler (actual HTTP request)
            // Это будет вызвано последним после всех middleware
            HttpMiddlewareDelegate pipeline = async (req, token) =>
            {
                // Финальный handler - должен быть реализован в UnityHttpClient
                throw new InvalidOperationException("HttpPipeline requires a final handler. Use SetFinalHandler() method.");
            };
            
            // Строим цепочку middleware в обратном порядке
            // Это обеспечивает правильный порядок выполнения
            for (int i = middlewareArray.Length - 1; i >= 0; i--)
            {
                var middleware = middlewareArray[i];
                var next = pipeline; // Сохраняем ссылку на следующий handler
                
                // Заменяем handler текущим middleware
                pipeline = async (req, token) => 
                {
                    try
                    {
                        return await middleware.ProcessAsync(req, next, token);
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибки middleware
                        UnityEngine.Debug.LogError($"[HttpPipeline] Middleware '{middleware.Name}' failed: {ex.Message}");
                        throw;
                    }
                };
            }
            
            // Выполняем построенную цепочку
            return await pipeline(request, ct);
        }

        /// <summary>
        /// Возвращает все middleware в pipeline
        /// </summary>
        /// <returns>Копия списка middleware</returns>
        public IReadOnlyList<IHttpMiddleware> GetMiddlewares()
        {
            lock (lockObj)
            {
                return middlewares.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Возвращает true если middleware с указанным именём существует в pipeline
        /// </summary>
        /// <param name="middlewareName">Имя middleware для проверки</param>
        /// <returns>True если middleware существует</returns>
        public bool HasMiddleware(string middlewareName)
        {
            lock (lockObj)
            {
                return middlewares.Any(m => m.Name == middlewareName);
            }
        }

        /// <summary>
        /// Устанавливает финальный handler для выполнения фактического HTTP запроса
        /// Используется в UnityHttpClient для реализации базового функционала
        /// </summary>
        /// <param name="finalHandler">Финальный handler для выполнения запроса</param>
        internal void SetFinalHandler(HttpMiddlewareDelegate finalHandler)
        {
            if (finalHandler == null)
            {
                throw new ArgumentNullException(nameof(finalHandler));
            }
            
            // Создаём приватный middleware для финального handler
            var finalMiddleware = new FinalHandlerMiddleware(finalHandler);
            
            lock (lockObj)
            {
                // Проверяем что финальный handler ещё не установлен
                if (middlewares.Any(m => m.Name == "FinalHandler"))
                {
                    // Заменяем существующий
                    var existing = middlewares.FirstOrDefault(m => m.Name == "FinalHandler");
                    middlewares.Remove(existing);
                }
                
                // Добавляем в конец цепочки
                middlewares.Add(finalMiddleware);
            }
        }

        /// <summary>
        /// Приватный middleware для финального handler
        /// </summary>
        private class FinalHandlerMiddleware : IHttpMiddleware
        {
            public string Name => "FinalHandler";
            
            private readonly HttpMiddlewareDelegate finalHandler;
            
            public FinalHandlerMiddleware(HttpMiddlewareDelegate finalHandler)
            {
                this.finalHandler = finalHandler;
            }
            
            public UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct)
            {
                // Игнорируем next и вызываем финальный handler напрямую
                return finalHandler(request, ct);
            }
        }
    }
}
