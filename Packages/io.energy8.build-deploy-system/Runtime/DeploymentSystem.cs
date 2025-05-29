using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Energy8.BuildDeploySystem
{
    public static class DeploymentSystem
    {
        public static event Action<string> OnDeployStarted;
        public static event Action<string, bool> OnDeployCompleted;
        public static event Action<string> OnDeployProgress;
        
        public static void Deploy(DeployConfiguration config, string buildPath)
        {
            if (config == null || !config.enabled)
            {
                UnityEngine.Debug.LogWarning("Deployment is not configured or disabled");
                return;
            }
            
            OnDeployStarted?.Invoke(config.host);
            
            try
            {
                OnDeployProgress?.Invoke("Preparing deployment...");
                ValidateDeploymentConfig(config);
                
                OnDeployProgress?.Invoke("Uploading files via SSH...");
                UploadViaSSH(config, buildPath);
                
                OnDeployCompleted?.Invoke(config.host, true);
                UnityEngine.Debug.Log($"Deployment completed successfully to {config.host}");
            }
            catch (Exception ex)
            {
                OnDeployCompleted?.Invoke(config.host, false);
                UnityEngine.Debug.LogError($"Deployment failed: {ex.Message}");
            }
        }
        
        private static void ValidateDeploymentConfig(DeployConfiguration config)
        {
            if (string.IsNullOrEmpty(config.host))
                throw new ArgumentException("Host is required for deployment");
            
            if (string.IsNullOrEmpty(config.username))
                throw new ArgumentException("Username is required for deployment");
            
            if (string.IsNullOrEmpty(config.keyPath) || !File.Exists(config.keyPath))
                throw new ArgumentException("Valid SSH key path is required for deployment");
            
            if (string.IsNullOrEmpty(config.remotePath))
                throw new ArgumentException("Remote path is required for deployment");
        }
        
        private static void UploadViaSSH(DeployConfiguration config, string buildPath)
        {
            if (!Directory.Exists(buildPath))
            {
                throw new DirectoryNotFoundException($"Build directory not found: {buildPath}");
            }
            
            // Используем scp для загрузки файлов
            string scpCommand = BuildScpCommand(config, buildPath);
            
            OnDeployProgress?.Invoke("Executing SCP command...");
            ExecuteCommand(scpCommand);
        }
        
        private static string BuildScpCommand(DeployConfiguration config, string buildPath)
        {
            // Экранируем пути для Windows
            string escapedKeyPath = config.keyPath.Replace("\\", "/");
            string escapedBuildPath = buildPath.Replace("\\", "/");
            
            return $"scp -i \"{escapedKeyPath}\" -P {config.port} -r \"{escapedBuildPath}\"/* {config.username}@{config.host}:{config.remotePath}";
        }
        
        private static void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();
                    
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Command failed with exit code {process.ExitCode}: {error}");
                    }
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        UnityEngine.Debug.Log($"SCP Output: {output}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute deployment command: {ex.Message}");
            }
        }
    }
}
