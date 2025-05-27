using System;
using UnityEngine;

namespace Energy8.Firebase.Core.Configuration.Models
{
    /// <summary>
    /// Firebase configuration for a specific environment and platform
    /// </summary>
    [Serializable]
    public class FirebaseEnvironmentConfig
    {
        [SerializeField] private FirebaseEnvironment environment;
        [SerializeField] private FirebasePlatform platform;
        [SerializeField] private TextAsset config;
        [SerializeField] private bool isEnabled = true;

        /// <summary>
        /// Configuration environment type
        /// </summary>
        public FirebaseEnvironment Environment
        {
            get => environment;
            set => environment = value;
        }

        /// <summary>
        /// Target platform for this configuration
        /// </summary>
        public FirebasePlatform Platform
        {
            get => platform;
            set => platform = value;
        }

        /// <summary>
        /// Firebase configuration file (JSON)
        /// </summary>
        public TextAsset Config
        {
            get => config;
            set => config = value;
        }

        /// <summary>
        /// Whether this configuration is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        /// <summary>
        /// Get configuration content as string
        /// </summary>
        public string ConfigText => config?.text;

        public FirebaseEnvironmentConfig()
        {
            environment = FirebaseEnvironment.Local;
            platform = FirebasePlatform.SDK;
            isEnabled = true;
        }

        public FirebaseEnvironmentConfig(FirebaseEnvironment env, FirebasePlatform plat, TextAsset configAsset = null)
        {
            environment = env;
            platform = plat;
            config = configAsset;
            isEnabled = true;
        }
    }
}