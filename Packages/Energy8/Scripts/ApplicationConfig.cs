using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy8
{
    [CreateAssetMenu(menuName = "Configs/CreateApplicationConfig")]
    public class ApplicationConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] List<IPConfig> _ipConfigs = new();
        [SerializeField] IPType _selectedIPType;
        [SerializeField] AuthType _selectedAuthType;

        [SerializeField] List<AuthConfig> _firebaseAuthConfigs;
        [SerializeField] List<AuthConfig> _firebaseWebAuthConfigs;

        public static IPType SelectedIPType
        {
            get
            {
                return Instance._selectedIPType;
            }
            set
            {
                Instance._selectedIPType = value;
            }
        }
        public static AuthType SelectedAuthType
        {
            get
            {
                return Instance._selectedAuthType;
            }
            set
            {
                Instance._selectedAuthType = value;
            }
        }
#endif

        private static ApplicationConfig _instance;
        public static ApplicationConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    UniTask.SwitchToMainThread();
                    _instance = Resources.Load<ApplicationConfig>("Configuration/ApplicationConfig");
                }
                return _instance;
            }
        }

        public static string SelectedIP => Instance._ipConfigs.First((c) => c.ipType == Instance._selectedIPType).ipAddress;
        public static string AuthConfig =>
#if UNITY_WEBGL && !UNITY_EDITOR
            Instance._firebaseWebAuthConfigs.First((c) => c.authType == Instance._selectedAuthType).config.text;
#else
            Instance._firebaseAuthConfigs.First((c) => c.authType == Instance._selectedAuthType).config.text;
#endif
    }

    [Serializable]
    public class IPConfig
    {
        public IPType ipType;
        public string ipAddress;
    }
    [Serializable]
    public class AuthConfig
    {
        public AuthType authType;
        public TextAsset config;
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
    public enum AuthType
    {
        Local,
        Debug,
        Production
    }
}