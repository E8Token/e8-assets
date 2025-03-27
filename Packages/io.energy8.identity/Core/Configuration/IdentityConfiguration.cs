using System.Collections.Generic;
using System.Linq;
using Energy8.Identity.Core.Configuration.Models;
using UnityEngine;

namespace Energy8.Identity.Core.Configuration
{
    [CreateAssetMenu(menuName = "Identity/Configuration")]
    public class IdentityConfiguration : ScriptableObject
    {
        [SerializeField]
        private List<IPConfig> ipConfigs = new() { new () {
            ipType = IPType.LocalPC,
            ipAddress = "http://localhost"
        } };
        [SerializeField] private List<AuthConfig> firebaseAuthConfigs = new();
        [SerializeField] private List<AuthConfig> firebaseWebAuthConfigs = new();

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
    }
}