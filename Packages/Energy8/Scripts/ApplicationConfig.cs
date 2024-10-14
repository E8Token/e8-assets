using System.Collections.Generic;
using UnityEngine;
namespace Energy8
{
    [CreateAssetMenu(menuName = "Configs/CreateApplicationConfig")]
    public class ApplicationConfig : ScriptableObject
    {
        [SerializeField] private string googleWebApi = "";
        [SerializeField] private List<IPConfig> ipConfigs = new List<IPConfig>();
        [SerializeField] private IPType selectedIPType; // Выбранный IPType

        private static ApplicationConfig _instance;

        public static ApplicationConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ApplicationConfig>("Configuration/ApplicationConfig");
                }
                return _instance;
            }
        }

        // Публичное статическое свойство для доступа к googleWebApi
        public static string GoogleWebAPI
        {
            get
            {
                return Instance.googleWebApi;
            }
            set
            {
                Instance.googleWebApi = value;
            }
        }

        // Получаем текущий IPConfig на основе выбранного IPType
        public static string SelectedIP
        {
            get
            {
                var config = Instance.ipConfigs.Find(c => c.ipType == Instance.selectedIPType);
                return config?.ipAddress ?? "localhost";
            }
        }

        // Устанавливаем выбранный IPType
        public static IPType SelectedIPType
        {
            get
            {
                return Instance.selectedIPType;
            }
            set
            {
                Instance.selectedIPType = value;
            }
        }
    }

    [System.Serializable]
    public class IPConfig
    {
        public IPType ipType;
        public string ipAddress;
    }

    public enum IPType
    {
        LocalPC,
        LocalNetwork,
        Debug,
        DebugTLS,
        Production,
        ProductionTLS
    }
}