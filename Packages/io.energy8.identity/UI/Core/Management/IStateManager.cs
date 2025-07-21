using System;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.UI.Core.Management
{
    /// <summary>
    /// Интерфейс для управления состоянием Identity системы
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Текущее состояние системы
        /// </summary>
        IdentityState CurrentState { get; }
        
        /// <summary>
        /// Событие изменения состояния (oldState, newState)
        /// </summary>
        event Action<IdentityState, IdentityState> StateChanged;
        
        /// <summary>
        /// Проверяет возможность перехода в новое состояние
        /// </summary>
        bool CanTransitionTo(IdentityState newState);
        
        /// <summary>
        /// Переход в новое состояние с валидацией
        /// </summary>
        void TransitionTo(IdentityState newState);
        
        /// <summary>
        /// Запуск начального потока системы
        /// </summary>
        UniTask StartInitialFlowAsync();
        
        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        void Dispose();
    }
}
