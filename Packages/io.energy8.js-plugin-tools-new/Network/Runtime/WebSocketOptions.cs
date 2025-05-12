using System.Collections.Generic;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Класс параметров для WebSocket соединения
    /// </summary>
    public class WebSocketOptions
    {
        /// <summary>
        /// Протоколы, поддерживаемые WebSocket соединением
        /// </summary>
        public string[] Protocols { get; set; }
        
        /// <summary>
        /// Заголовки, которые будут отправлены при создании соединения
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        
        /// <summary>
        /// Таймаут соединения в миллисекундах
        /// </summary>
        public int? ConnectionTimeout { get; set; }
        
        /// <summary>
        /// Автоматическая переотправка сообщений при переподключении
        /// </summary>
        public bool AutoReconnect { get; set; }
        
        /// <summary>
        /// Максимальное количество попыток переподключения
        /// </summary>
        public int MaxReconnectAttempts { get; set; }
        
        /// <summary>
        /// Интервал между попытками переподключения в миллисекундах
        /// </summary>
        public int ReconnectInterval { get; set; }
        
        /// <summary>
        /// Создает новый экземпляр WebSocketOptions с настройками по умолчанию
        /// </summary>
        public WebSocketOptions()
        {
            Headers = new Dictionary<string, string>();
            AutoReconnect = false;
            MaxReconnectAttempts = 3;
            ReconnectInterval = 5000;
        }
    }
}