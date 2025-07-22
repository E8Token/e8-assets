using System;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{    [Serializable]
    public class WebGLSettings
    {
        [Header("Multi-Format Texture Compression")]
        [Tooltip("Build additional DXT format for desktop browsers")]
        [SerializeField] private bool buildDXT = false;
        [Tooltip("Build additional ASTC format for mobile browsers")]
        [SerializeField] private bool buildASTC = false;
        [Tooltip("Build additional ETC2 format for mobile browsers")]
        [SerializeField] private bool buildETC2 = false;

        [Header("WebGL Compression Algorithms")]
        [Tooltip("Select compression algorithms for WebGL build output")]
        [SerializeField] private System.Collections.Generic.List<CompressionAlgorithm> compressionAlgorithms = new System.Collections.Generic.List<CompressionAlgorithm>();

        public bool BuildDXT
        {
            get => buildDXT;
            set => buildDXT = value;
        }

        public bool BuildASTC
        {
            get => buildASTC;
            set => buildASTC = value;
        }

        public bool BuildETC2
        {
            get => buildETC2;
            set => buildETC2 = value;
        }

        /// <summary>Selected compression algorithms for WebGL build output.</summary>
        public System.Collections.Generic.List<CompressionAlgorithm> CompressionAlgorithms
        {
            get => compressionAlgorithms;
            set => compressionAlgorithms = value;
        }

        public string[] GetTextureFormatsToBuild()
        {
            var formats = new System.Collections.Generic.List<string>();
            
            if (buildDXT) formats.Add("DXT");
            if (buildASTC) formats.Add("ASTC");
            if (buildETC2) formats.Add("ETC2");
            
            return formats.ToArray();
        }        public bool HasMultipleFormats()
        {
            int count = 0;
            if (buildDXT) count++;
            if (buildASTC) count++;
            if (buildETC2) count++;
            return count > 0;
        }

        /// <summary>Gets the selected compression algorithms as strings.</summary>
        public string[] GetCompressionAlgorithmNames()
        {
            var names = new System.Collections.Generic.List<string>();
            foreach (var algo in compressionAlgorithms)
            {
                names.Add(algo.ToString());
            }
            return names.ToArray();
        }
    }

    [Serializable]
    public class AndroidSettings
    {
        [Header("Android Build Options")]
        [SerializeField] private bool buildAAB = false;
        [SerializeField] private bool buildAPK = true;
        [SerializeField] private string keystorePath;
        [SerializeField] private bool useCustomKeystore = false;
        
        [Header("Architecture")]
        [SerializeField] private bool buildARM64 = true;
        [SerializeField] private bool buildARMv7 = false;

        public bool BuildAAB
        {
            get => buildAAB;
            set => buildAAB = value;
        }

        public bool BuildAPK
        {
            get => buildAPK;
            set => buildAPK = value;
        }

        public string KeystorePath
        {
            get => keystorePath;
            set => keystorePath = value;
        }

        public bool UseCustomKeystore
        {
            get => useCustomKeystore;
            set => useCustomKeystore = value;
        }

        public bool BuildARM64
        {
            get => buildARM64;
            set => buildARM64 = value;
        }

        public bool BuildARMv7
        {
            get => buildARMv7;
            set => buildARMv7 = value;
        }
    }

    [Serializable]
    public class IOSSettings
    {
        [Header("iOS Build Options")]
        [SerializeField] private string developmentTeamId;
        [SerializeField] private bool automaticSigning = true;
        [SerializeField] private string provisioningProfilePath;
        
        [Header("Architecture")]
        [SerializeField] private bool buildSimulator = false;
        [SerializeField] private bool buildDevice = true;

        public string DevelopmentTeamId
        {
            get => developmentTeamId;
            set => developmentTeamId = value;
        }

        public bool AutomaticSigning
        {
            get => automaticSigning;
            set => automaticSigning = value;
        }

        public string ProvisioningProfilePath
        {
            get => provisioningProfilePath;
            set => provisioningProfilePath = value;
        }

        public bool BuildSimulator
        {
            get => buildSimulator;
            set => buildSimulator = value;
        }

        public bool BuildDevice
        {
            get => buildDevice;
            set => buildDevice = value;
        }
    }

    [Serializable]
    public class StandaloneSettings
    {
        [Header("Standalone Options")]
        [SerializeField] private bool createInstaller = false;
        [SerializeField] private string installerName;
        [SerializeField] private bool includeDebugSymbols = false;

        public bool CreateInstaller
        {
            get => createInstaller;
            set => createInstaller = value;
        }

        public string InstallerName
        {
            get => installerName;
            set => installerName = value;
        }

        public bool IncludeDebugSymbols
        {
            get => includeDebugSymbols;
            set => includeDebugSymbols = value;
        }
    }

    /// <summary>
    /// Defines the available deployment methods for build artifacts.
    /// </summary>
    [Serializable]
    public enum DeployMethod
    {
        /// <summary>
        /// No deployment method selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Deploy using File Transfer Protocol.
        /// </summary>
        FTP = 1,
        /// <summary>
        /// Deploy using SSH File Transfer Protocol (secure).
        /// </summary>
        SFTP = 2,
        /// <summary>
        /// Deploy by copying files to a local directory.
        /// </summary>
        LocalCopy = 3
    }

    /// <summary>
    /// Defines the available authentication methods for remote deployment.
    /// </summary>
    [Serializable]
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Authenticate using username and password.
        /// </summary>
        Password = 0,
        /// <summary>
        /// Authenticate using a private key file.
        /// </summary>
        PrivateKey = 1
    }

    /// <summary>
    /// Defines the available compression algorithms for WebGL builds.
    /// </summary>
    [Serializable]
    public enum CompressionAlgorithm
    {
        /// <summary>
        /// No compression applied.
        /// </summary>
        None,
        /// <summary>
        /// Brotli compression algorithm (high compression ratio).
        /// </summary>
        Brotli,
        /// <summary>
        /// Gzip compression algorithm (widely supported).
        /// </summary>
        Gzip
    }

    [Serializable]
    public class DeploySettings
    {
        [Header("Deploy Configuration")]
        [SerializeField] private bool enableDeploy = false;
        [SerializeField] private bool alwaysDeploy = false;
        [SerializeField] private DeployMethod deployMethod = DeployMethod.FTP;

        [Header("Server Settings")]
        [SerializeField] private string serverHost = "";
        [SerializeField] private int serverPort = 21;
        [SerializeField] private string remotePath = "/";

        [Header("Authentication")]
        [SerializeField] private AuthenticationMethod authMethod = AuthenticationMethod.Password;
        [SerializeField] private string username = "";
        [SerializeField] private string password = "";
        [SerializeField] private string privateKeyPath = "";
        [SerializeField] private string privateKeyPassphrase = "";

        [Header("Deploy Options")]
        [SerializeField] private bool deleteExistingFiles = false;
        [SerializeField] private bool createBackup = true;
        [SerializeField] private bool deployZipOnly = false;

        [Header("Local Copy Deploy")]
        [SerializeField] private string localCopyTargetPath = "";

        public bool EnableDeploy
        {
            get => enableDeploy;
            set => enableDeploy = value;
        }

        public bool AlwaysDeploy
        {
            get => alwaysDeploy;
            set => alwaysDeploy = value;
        }

        public DeployMethod Method
        {
            get => deployMethod;
            set => deployMethod = value;
        }

        public string ServerHost
        {
            get => serverHost;
            set => serverHost = value;
        }

        public int ServerPort
        {
            get => serverPort;
            set => serverPort = value;
        }

        public string RemotePath
        {
            get => remotePath;
            set => remotePath = value;
        }

        public AuthenticationMethod AuthMethod
        {
            get => authMethod;
            set => authMethod = value;
        }

        public string Username
        {
            get => username;
            set => username = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public string PrivateKeyPath
        {
            get => privateKeyPath;
            set => privateKeyPath = value;
        }

        public string PrivateKeyPassphrase
        {
            get => privateKeyPassphrase;
            set => privateKeyPassphrase = value;
        }

        public bool DeleteExistingFiles
        {
            get => deleteExistingFiles;
            set => deleteExistingFiles = value;
        }

        public bool CreateBackup
        {
            get => createBackup;
            set => createBackup = value;
        }

        public bool DeployZipOnly
        {
            get => deployZipOnly;
            set => deployZipOnly = value;
        }

        public string LocalCopyTargetPath
        {
            get => localCopyTargetPath;
            set => localCopyTargetPath = value;
        }

        public bool IsValid()
        {
            if (!enableDeploy) return true;

            if (deployMethod == DeployMethod.LocalCopy)
                return !string.IsNullOrEmpty(localCopyTargetPath);

            if (string.IsNullOrEmpty(serverHost) || string.IsNullOrEmpty(username))
                return false;

            if (authMethod == AuthenticationMethod.Password && string.IsNullOrEmpty(password))
                return false;

            if (authMethod == AuthenticationMethod.PrivateKey && string.IsNullOrEmpty(privateKeyPath))
                return false;

            return true;
        }
    }
}
