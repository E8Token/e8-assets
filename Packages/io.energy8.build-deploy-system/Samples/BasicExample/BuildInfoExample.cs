using UnityEngine;
using Energy8.BuildDeploySystem;

namespace Energy8.BuildDeploySystem.Samples
{
    /// <summary>
    /// Пример использования BuildInfo компонента
    /// </summary>
    public class BuildInfoExample : MonoBehaviour
    {
        [Header("Build Info Settings")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode displayKey = KeyCode.F1;
        
        private BuildInfo buildInfo;

        private void Start()
        {
            buildInfo = GetComponent<BuildInfo>();
            if (buildInfo == null)
            {
                buildInfo = gameObject.AddComponent<BuildInfo>();
            }

            if (showOnStart)
            {
                DisplayInfo();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(displayKey))
            {
                DisplayInfo();
            }
        }

        private void DisplayInfo()
        {
            if (buildInfo != null)
            {
                buildInfo.DisplayBuildInfo();
                
                // Также можно получить информацию в виде строки
                string info = buildInfo.GetBuildInfoString();
                Debug.Log($"Build Info String: {info}");
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 20), $"Press {displayKey} to show build info");
            
            if (buildInfo != null)
            {
                string info = buildInfo.GetBuildInfoString();
                GUI.Label(new Rect(10, 30, 400, 20), info);
            }
        }
    }
}
