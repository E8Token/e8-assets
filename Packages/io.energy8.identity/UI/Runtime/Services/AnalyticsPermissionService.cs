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
        private readonly bool debugLogging;

        public AnalyticsPermissionService(ICanvasManager canvasManager, bool debugLogging = false)
        {
            this.canvasManager = canvasManager;
            this.debugLogging = debugLogging;
        }

        public bool IsAnalyticsPermissionRequested => 
            PlayerPrefs.HasKey(PlayerPrefsKeys.ANALYTICS_PERMISSION_REQUESTED);

        public bool HasAnalyticsPermission => 
            PlayerPrefs.GetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION, 0) == 1;

        public bool ShouldShowAnalyticsPermissionRequest => 
            !IsAnalyticsPermissionRequested;

        private readonly IAnalyticsFlowManager analyticsFlowManager;

        public AnalyticsPermissionService(ICanvasManager canvasManager, bool debugLogging = false, IAnalyticsFlowManager analyticsFlowManager = null)
        {
            this.canvasManager = canvasManager;
            this.debugLogging = debugLogging;
            this.analyticsFlowManager = analyticsFlowManager;
        }

        public async UniTask<bool> RequestAnalyticsPermissionAsync(CancellationToken ct = default)
        {
            if (debugLogging)
                Debug.Log("Requesting analytics permission from user");

            try
            {
                if (debugLogging)
                    Debug.Log("Opening AnalyticsView for permission request via AnalyticsFlowManager");

                if (analyticsFlowManager == null)
                    throw new System.InvalidOperationException("AnalyticsFlowManager is not set in AnalyticsPermissionService");

                var isAllowed = await analyticsFlowManager.ShowAnalyticsFlowAsync(ct);

                if (debugLogging)
                    Debug.Log("AnalyticsView closed via AnalyticsFlowManager");

                // Сохраняем результат
                SaveAnalyticsPermission(isAllowed);
                // Отмечаем, что запрос был сделан
                PlayerPrefs.SetInt(PlayerPrefsKeys.ANALYTICS_PERMISSION_REQUESTED, 1);
                PlayerPrefs.Save();

                if (debugLogging)
                    Debug.Log($"Analytics permission result: {isAllowed}");

                return isAllowed;
            }
            catch (System.Exception ex)
            {
                if (debugLogging)
                    Debug.LogError($"Error requesting analytics permission: {ex.Message}");
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
            
            if (debugLogging)
                Debug.Log($"Analytics permission saved: {granted}");
        }
    }
}
