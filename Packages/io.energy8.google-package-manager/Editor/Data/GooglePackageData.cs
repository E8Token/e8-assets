using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.GooglePackageManager.Data
{    [Serializable]
    public class GooglePackageInfo
    {
        public string packageName;
        public string displayName;
        public string version;
        public string description;
        public string category;
        public string minimumUnityVersion;
        public string publishDate;
        public string downloadUrlTgz;
        public bool isInstalled;
        public string installedVersion;
        public bool hasUpdate;
        public List<GooglePackageVersion> availableVersions; // Новое поле для всех версий
        
        public GooglePackageInfo()
        {
            packageName = "";
            displayName = "";
            version = "";
            description = "";
            category = "";
            minimumUnityVersion = "";
            publishDate = "";
            downloadUrlTgz = "";
            isInstalled = false;
            installedVersion = "";
            hasUpdate = false;
            availableVersions = new List<GooglePackageVersion>();
        }
    }
    
    [Serializable]
    public class GooglePackageVersion
    {
        public string version;
        public string publishDate;
        public string minimumUnityVersion;
        public string downloadUrlTgz;
        public string downloadUrlUnityPackage;
        public string dependencies;
        
        public GooglePackageVersion()
        {
            version = "";
            publishDate = "";
            minimumUnityVersion = "";
            downloadUrlTgz = "";
            downloadUrlUnityPackage = "";
            dependencies = "";
        }
    }
    
    [Serializable]
    public class GooglePackageCategory
    {
        public string name;
        public string displayName;
        public List<GooglePackageInfo> packages;
        
        public GooglePackageCategory()
        {
            name = "";
            displayName = "";
            packages = new List<GooglePackageInfo>();
        }
        
        public GooglePackageCategory(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
            packages = new List<GooglePackageInfo>();
        }
    }
    
    [Serializable]
    public class GooglePackageDatabase
    {
        public List<GooglePackageCategory> categories;
        public DateTime lastUpdateCheck;
        
        public GooglePackageDatabase()
        {
            categories = new List<GooglePackageCategory>();
            lastUpdateCheck = DateTime.MinValue;
        }
    }
}
