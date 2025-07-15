using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Energy8.Identity.Configuration.Core
{
    [CreateAssetMenu(menuName = "Identity/Configuration")]
    public class IdentityConfiguration : ScriptableObject
    {
        [SerializeField] private List<IPConfig> ipConfigs = new() { new () {
            ipType = IPType.LocalPC,
            ipAddress = "http://localhost"
        } };
        [SerializeField] private List<AuthConfig> firebaseAuthConfigs = new();
        [SerializeField] private List<AuthConfig> firebaseWebAuthConfigs = new();

        [Header("Analytics Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private bool trackUserActions = true;
        [SerializeField] private bool trackErrors = true;
        [SerializeField] private bool trackPerformance = false;

        [SerializeField] private IPType selectedIPType;
        [SerializeField] private AuthType selectedAuthType;

        private const string ConfigPath = "Identity/Configuration/IdentityConfiguration";

        private static IdentityConfiguration instance;
        public static IdentityConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<IdentityConfiguration>(ConfigPath);
                }
                return instance;
            }
        }

        public static IPType SelectedIPType
        {
            get => Instance.selectedIPType;
            set => Instance.selectedIPType = value;
        }

        public static AuthType SelectedAuthType
        {
            get => Instance.selectedAuthType;
            set => Instance.selectedAuthType = value;
        }

        public static string SelectedIP => Instance.ipConfigs
            .First(c => c.ipType == Instance.selectedIPType).ipAddress;

        public static string AuthConfig =>
#if UNITY_WEBGL && !UNITY_EDITOR
            Instance.firebaseWebAuthConfigs
#else
            Instance.firebaseAuthConfigs
#endif
            .First(c => c.authType == Instance.selectedAuthType).config.text;

        // Analytics Configuration Properties
        public static bool EnableAnalytics => Instance.enableAnalytics;
        public static bool EnableDebugLogging => Instance.enableDebugLogging;
        public static bool TrackUserActions => Instance.trackUserActions;
        public static bool TrackErrors => Instance.trackErrors;
        public static bool TrackPerformance => Instance.trackPerformance;

        // IConfigurationProvider implementation
        public bool IsValid => ValidationErrors.Length == 0;
        
        public string[] ValidationErrors
        {
            get
            {
                var errors = new List<string>();
                
                if (ipConfigs == null || ipConfigs.Count == 0)
                    errors.Add("No IP configurations defined");
                    
                if (firebaseAuthConfigs == null || firebaseAuthConfigs.Count == 0)
                    errors.Add("No Firebase auth configurations defined");
                    
                if (firebaseWebAuthConfigs == null || firebaseWebAuthConfigs.Count == 0)
                    errors.Add("No Firebase WebGL auth configurations defined");
                    
                if (!ipConfigs.Any(c => c.ipType == selectedIPType))
                    errors.Add($"Selected IP type '{selectedIPType}' not found in configurations");
                    
                if (!firebaseAuthConfigs.Any(c => c.authType == selectedAuthType) && 
                    !firebaseWebAuthConfigs.Any(c => c.authType == selectedAuthType))
                    errors.Add($"Selected auth type '{selectedAuthType}' not found in configurations");
                
                return errors.ToArray();
            }
        }
    }
}