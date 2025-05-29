using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Energy8.BuildDeploySystem
{
    [Serializable]
    public class BuildConfiguration : ScriptableObject
    {
        [Header("General Settings")]
        public string configName = "Default";
        public string buildProfile = "Development";
        public bool autoIncrement = true;
        
        [Header("Version Settings")]
        public string version = "1.0.0";
        public VersionIncrementType versionIncrementType = VersionIncrementType.Auto;
        
        [Header("Platform Settings")]
        public BuildTargetType buildTarget = BuildTargetType.WebGL;
        public string bundleId = "io.energy8.game";
        
        [Header("WebGL Texture Compression")]
        public List<TextureCompressionMethod> webglCompressionMethods = new List<TextureCompressionMethod>();
        
        [Header("Deploy Settings")]
        public bool autoDeployOnSuccess = false;
        public DeployConfiguration deployConfig = new DeployConfiguration();
        
        public void IncrementVersion()
        {
            var versionParts = version.Split('.');
            if (versionParts.Length != 3)
            {
                version = "1.0.0";
                return;
            }
            
            var major = int.Parse(versionParts[0]);
            var minor = int.Parse(versionParts[1]);
            var patch = int.Parse(versionParts[2]);
            
            switch (versionIncrementType)
            {
                case VersionIncrementType.Major:
                    major++;
                    minor = 0;
                    patch = 0;
                    break;
                case VersionIncrementType.Minor:
                    minor++;
                    patch = 0;
                    break;
                case VersionIncrementType.Auto:
                    patch++;
                    break;
            }
            
            version = $"{major}.{minor}.{patch}";
        }
    }
    
    [Serializable]
    public class DeployConfiguration
    {
        [Header("SSH Deploy Settings")]
        public bool enabled = false;
        public string host = "";
        public string username = "";
        public string keyPath = "";
        public string remotePath = "";
        public int port = 22;
    }
      public enum VersionIncrementType
    {
        Major,
        Minor,
        Auto
    }
    
    public enum TextureCompressionMethod
    {
        DXT = 1,
        ASTC = 2,
        ETC2 = 3
    }
    
    public enum BuildTargetType
    {
        WebGL,
        Android,
        iOS,
        StandaloneWindows,
        StandaloneWindows64,
        StandaloneOSX,
        StandaloneLinux64
    }
}
