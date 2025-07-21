using System;

namespace Energy8.Identity.UI.Runtime.Services
{
    /// <summary>
    /// Простейшая реализация IUpdateService (заглушка)
    /// </summary>
    public class UpdateService : IUpdateService
    {
        public bool HasUpdate { get; set; }

        public UpdateService(bool hasUpdate = false)
        {
            HasUpdate = hasUpdate;
        }
    }
}
