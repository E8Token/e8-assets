using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Energy8.Identity.Shared.Core.Constants;
using Energy8.Identity.UI.Runtime.Management.Flows;
using Energy8.Identity.UI.Core.Management;

namespace Energy8.Identity.UI.Runtime.Services
{
    /// <summary>
    /// Реализация сервиса для управления разрешениями на аналитику
    /// </summary>
    public class AnalyticsPermissionService : IAnalyticsPermissionService
    {
        private readonly ICanvasManager canvasManager;
        private readonly IAnalyticsFlowManager analyticsFlowManager;

        public AnalyticsPermissionService(ICanvasManager canvasManager, IAnalyticsFlowManager analyticsFlowManager = null)
        {
            this.canvasManager = canvasManager;
            this.analyticsFlowManager = analyticsFlowManager;
        }

        public bool IsAnalyticsPermissionRequested => 
            PlayerPrefs.HasKey(PlayerPrefsKeys.ANALYTICS_PERMISSION_REQUESTED);

        public bool HasAnalyticsPermission => 
            PlayerPrefs.GetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION, 0) == 1;

        public bool ShouldShowAnalyticsPermissionRequest => 
            !IsAnalyticsPermissionRequested;

        public async UniTask<bool> RequestAnalyticsPermissionAsync(CancellationToken ct = default)
        {
            try
            {
                if (analyticsFlowManager == null)
                    throw new System.InvalidOperationException("AnalyticsFlowManager is not set in AnalyticsPermissionService");

                var isAllowed = await analyticsFlowManager.ShowAnalyticsFlowAsync(ct);

                // Сохраняем результат
                SaveAnalyticsPermission(isAllowed);
                // Отмечаем, что запрос был сделан
                PlayerPrefs.SetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION_REQUESTED, 1);
                PlayerPrefs.Save();

                return isAllowed;
            }
            catch (System.Exception ex)
            {
                // В случае ошибки считаем, что разрешение не дано
                SaveAnalyticsPermission(false);
                PlayerPrefs.SetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION_REQUESTED, 1);
                PlayerPrefs.Save();
                return false;
            }
        }

        public void SaveAnalyticsPermission(bool granted)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION, granted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
