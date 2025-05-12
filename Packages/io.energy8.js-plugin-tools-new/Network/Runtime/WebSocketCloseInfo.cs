namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Класс, содержащий информацию о закрытии WebSocket соединения
    /// </summary>
    public class WebSocketCloseInfo
    {
        /// <summary>
        /// Код закрытия соединения
        /// </summary>
        public int Code { get; }
        
        /// <summary>
        /// Причина закрытия соединения
        /// </summary>
        public string Reason { get; }
        
        /// <summary>
        /// Было ли закрытие выполнено чисто (без ошибок)
        /// </summary>
        public bool WasClean { get; }
        
        /// <summary>
        /// Создает новый экземпляр WebSocketCloseInfo
        /// </summary>
        /// <param name="code">Код закрытия</param>
        /// <param name="reason">Причина закрытия</param>
        /// <param name="wasClean">Признак чистого закрытия</param>
        public WebSocketCloseInfo(int code, string reason, bool wasClean)
        {
            Code = code;
            Reason = reason;
            WasClean = wasClean;
        }
    }
}