namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Перечисление состояний WebSocket соединения
    /// </summary>
    public enum WebSocketState
    {
        /// <summary>
        /// Соединение закрыто или еще не было установлено
        /// </summary>
        Closed = 0,
        
        /// <summary>
        /// Процесс установки соединения
        /// </summary>
        Connecting = 1,
        
        /// <summary>
        /// Соединение открыто и готово к обмену данными
        /// </summary>
        Open = 2,
        
        /// <summary>
        /// Процесс закрытия соединения
        /// </summary>
        Closing = 3
    }
}