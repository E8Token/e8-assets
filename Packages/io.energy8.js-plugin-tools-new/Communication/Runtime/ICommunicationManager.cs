using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// Основной интерфейс для управления коммуникацией между Unity и JavaScript
    /// </summary>
    public interface ICommunicationManager
    {
        /// <summary>
        /// Инициализирует коммуникационный модуль
        /// </summary>
        /// <param name="core">Экземпляр ядра плагина</param>
        void Initialize(IPluginCore core);

        /// <summary>
        /// Отправляет данные в JavaScript
        /// </summary>
        /// <typeparam name="T">Тип данных для отправки</typeparam>
        /// <param name="channel">Канал, по которому отправляются данные</param>
        /// <param name="data">Данные для отправки</param>
        void Send<T>(string channel, T data);

        /// <summary>
        /// Отправляет данные в JavaScript и ожидает ответа
        /// </summary>
        /// <typeparam name="TRequest">Тип отправляемых данных</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
        /// <param name="channel">Канал, по которому отправляются данные</param>
        /// <param name="data">Данные для отправки</param>
        /// <returns>Ответ от JavaScript</returns>
        Task<TResponse> SendAsync<TRequest, TResponse>(string channel, TRequest data);

        /// <summary>
        /// Регистрирует обработчик для получения данных от JavaScript
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="channel">Канал, по которому приходят данные</param>
        /// <param name="handler">Обработчик данных</param>
        void RegisterHandler<T>(string channel, Action<T> handler);

        /// <summary>
        /// Отменяет регистрацию обработчика для указанного канала
        /// </summary>
        /// <param name="channel">Канал, для которого нужно отменить регистрацию</param>
        void UnregisterHandler(string channel);

        /// <summary>
        /// Проверяет, инициализирован ли коммуникационный модуль
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Событие, возникающее при инициализации модуля
        /// </summary>
        event Action OnInitialized;
    }
}