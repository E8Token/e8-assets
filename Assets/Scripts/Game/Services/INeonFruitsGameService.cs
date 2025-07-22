using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Game.Core.Services;
using Game.Dto;

namespace Game.Services
{
    /// <summary>
    /// Расширенный интерфейс для игровых операций NeonFruits слота
    /// Наследуется от базового IGameService и добавляет специфичные методы
    /// </summary>
    public interface INeonFruitsGameService : IGameService
    {
        /// <summary>
        /// Инициализирует игровую сессию NeonFruits
        /// </summary>
        /// <param name="sessionId">ID сессии</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Статус инициализированной игры</returns>
        UniTask<NFGameStatusResponseDto> InitializeGameAsync(string sessionId, CancellationToken ct);
        
        /// <summary>
        /// Выполняет спин в слоте
        /// </summary>
        /// <typeparam name="TSpinRequest">Тип запроса спина</typeparam>
        /// <typeparam name="TSpinResponse">Тип ответа спина</typeparam>
        /// <param name="request">Параметры спина</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Результат спина</returns>
        UniTask<TSpinResponse> SpinAsync<TSpinRequest, TSpinResponse>(TSpinRequest request, CancellationToken ct)
            where TSpinRequest : SpinRequestDto
            where TSpinResponse : GameStatusResponseDto;
        
        /// <summary>
        /// Активирует бонусную игру
        /// </summary>
        /// <param name="sessionId">ID сессии</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Статус после активации бонуса</returns>
        UniTask<NFGameStatusResponseDto> ActivateBonusAsync(string sessionId, CancellationToken ct);
        
        /// <summary>
        /// Получает текущий статус игры
        /// </summary>
        /// <param name="sessionId">ID сессии</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Текущий статус игры</returns>
        UniTask<NFGameStatusResponseDto> GetGameStatusAsync(string sessionId, CancellationToken ct);
        
        /// <summary>
        /// Завершает игровую сессию
        /// </summary>
        /// <param name="sessionId">ID сессии</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Финальный статус игры</returns>
        UniTask<NFGameStatusResponseDto> EndGameSessionAsync(string sessionId, CancellationToken ct);
    }
}
