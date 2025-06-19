using UnityEngine;

namespace Energy8.BuildDeploySystem
{
    /// <summary>
    /// Простой компонент для демонстрации работы с пакетом во время выполнения
    /// </summary>
    public class BuildInfo : MonoBehaviour
    {
        [SerializeField] private bool showBuildInfoOnStart = true;
        
        private void Start()
        {
            if (showBuildInfoOnStart)
            {
                DisplayBuildInfo();
            }
        }

        /// <summary>
        /// Отображает информацию о текущей сборке
        /// </summary>
        public void DisplayBuildInfo()
        {
            Debug.Log("=== BUILD INFO ===");
            Debug.Log($"Application Version: {Application.version}");
            Debug.Log($"Unity Version: {Application.unityVersion}");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Build GUID: {Application.buildGUID}");
            Debug.Log($"Data Path: {Application.dataPath}");
            Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
            Debug.Log($"Development Build: {Debug.isDebugBuild}");
            Debug.Log("==================");
        }

        /// <summary>
        /// Получает информацию о текущей сборке в виде строки
        /// </summary>
        /// <returns>Строка с информацией о сборке</returns>
        public string GetBuildInfoString()
        {
            return $"Version: {Application.version} | Platform: {Application.platform} | Debug: {Debug.isDebugBuild}";
        }
    }
}
