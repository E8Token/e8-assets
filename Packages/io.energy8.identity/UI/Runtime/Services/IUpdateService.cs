using System;

namespace Energy8.Identity.UI.Runtime.Services
{
    /// <summary>
    /// Сервис для проверки необходимости обновления приложения (заглушка)
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Есть ли обновление (заглушка)
        /// </summary>
        bool HasUpdate { get; set; }
    }
}
