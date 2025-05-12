using System;
using System.Threading.Tasks;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Класс для управления WebSocket соединением
    /// </summary>
    public class WebSocketConnection
    {
        /// <summary>
        /// Текущее состояние соединения
        /// </summary>
        public WebSocketState State { get; private set; }

        /// <summary>
        /// URL, по которому установлено соединение
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Событие, срабатывающее при получении сообщения
        /// </summary>
        public event Action<string> OnMessage;

        /// <summary>
        /// Событие, срабатывающее при закрытии соединения
        /// </summary>
        public event Action<WebSocketCloseInfo> OnClose;

        /// <summary>
        /// Событие, срабатывающее при возникновении ошибки
        /// </summary>
        public event Action<string> OnError;

        /// <summary>
        /// Событие, срабатывающее при открытии соединения
        /// </summary>
        public event Action OnOpen;

        /// <summary>
        /// Объект, который используется для взаимодействия с WebSocket API в браузере
        /// </summary>
        private readonly object _webSocketInstance;

        /// <summary>
        /// Создает новый экземпляр WebSocketConnection
        /// </summary>
        /// <param name="webSocketInstance">Объект для взаимодействия с WebSocket API в браузере</param>
        /// <param name="url">URL соединения</param>
        public WebSocketConnection(object webSocketInstance, string url)
        {
            _webSocketInstance = webSocketInstance ?? throw new ArgumentNullException(nameof(webSocketInstance));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            State = WebSocketState.Connecting;
        }

        /// <summary>
        /// Отправляет строковое сообщение через WebSocket соединение
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <returns>True, если сообщение успешно отправлено</returns>
        public async Task<bool> SendAsync(string message)
        {
            if (State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Cannot send message: WebSocket is not in OPEN state");
            }

            // Реализация будет зависеть от конкретной имплементации NetworkService
            // Эта заглушка будет заменена реальной реализацией
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Отправляет бинарные данные через WebSocket соединение
        /// </summary>
        /// <param name="data">Данные для отправки</param>
        /// <returns>True, если данные успешно отправлены</returns>
        public async Task<bool> SendBinaryAsync(byte[] data)
        {
            if (State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Cannot send binary data: WebSocket is not in OPEN state");
            }

            // Реализация будет зависеть от конкретной имплементации NetworkService
            // Эта заглушка будет заменена реальной реализацией
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Закрывает WebSocket соединение
        /// </summary>
        /// <param name="code">Код закрытия</param>
        /// <param name="reason">Причина закрытия</param>
        /// <returns>True, если соединение успешно закрыто</returns>
        public async Task<bool> CloseAsync(int code = 1000, string reason = "Normal closure")
        {
            if (State == WebSocketState.Closed || State == WebSocketState.Closing)
            {
                return true; // Уже закрыто или закрывается
            }

            State = WebSocketState.Closing;

            // Реализация будет зависеть от конкретной имплементации NetworkService
            // Эта заглушка будет заменена реальной реализацией
            State = WebSocketState.Closed;
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Вызывается при получении сообщения от WebSocket
        /// </summary>
        /// <param name="message">Полученное сообщение</param>
        internal void HandleMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        /// <summary>
        /// Вызывается при открытии WebSocket соединения
        /// </summary>
        internal void HandleOpen()
        {
            State = WebSocketState.Open;
            OnOpen?.Invoke();
        }

        /// <summary>
        /// Вызывается при закрытии WebSocket соединения
        /// </summary>
        /// <param name="closeInfo">Информация о закрытии соединения</param>
        internal void HandleClose(WebSocketCloseInfo closeInfo)
        {
            State = WebSocketState.Closed;
            OnClose?.Invoke(closeInfo);
        }

        /// <summary>
        /// Вызывается при возникновении ошибки в WebSocket соединении
        /// </summary>
        /// <param name="error">Описание ошибки</param>
        internal void HandleError(string error)
        {
            OnError?.Invoke(error);
        }
    }
}