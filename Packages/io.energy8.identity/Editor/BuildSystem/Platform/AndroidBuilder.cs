#if UNITY_EDITOR
using System.IO;
using Energy8.BuildSystem.Configuration;
using Energy8.BuildSystem.Utils;
using UnityEditor;
using UnityEngine;

namespace Energy8.BuildSystem.Platform
{
    public static class AndroidBuilder
    {
        public static void ConfigureAndroidSettings(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Configuring Android publishing settings...");

            PlayerSettings.Android.useCustomKeystore = config.useCustomKeystore;
            PlayerSettings.Android.minifyRelease = config.minifyType == MinifyType.Release;
            PlayerSettings.Android.minifyDebug = config.minifyType == MinifyType.Debug;

            if (config.useCustomKeystore)
            {
                if (!File.Exists(config.keystoreSettingsFile))
                {
                    logger.Log("Keystore settings file not found. Using default keystore.");
                    PlayerSettings.Android.useCustomKeystore = false;
                    return;
                }

                string json = File.ReadAllText(config.keystoreSettingsFile);
                KeystoreConfig keystoreConfig = JsonUtility.FromJson<KeystoreConfig>(json);

                // Set keystore parameters
                PlayerSettings.Android.keystoreName = keystoreConfig.keystorePath;
                PlayerSettings.Android.keystorePass = keystoreConfig.keystorePassword;
                PlayerSettings.Android.keyaliasName = keystoreConfig.keyAlias;
                PlayerSettings.Android.keyaliasPass = keystoreConfig.keyAliasPassword;

                logger.Log("Custom keystore settings applied.");
            }
        }
    }
}
#endif