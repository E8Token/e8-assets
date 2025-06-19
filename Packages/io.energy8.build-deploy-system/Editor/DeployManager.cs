using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public static class DeployManager
    {
        public static async Task<bool> DeployBuild(BuildConfiguration config, string buildPath)
        {
            var deploySettings = config.DeploySettings;
            
            if (!deploySettings.EnableDeploy)
            {
                Debug.Log("Deploy is disabled for this configuration.");
                return true;
            }
            
            if (!deploySettings.IsValid())
            {
                Debug.LogError("Deploy settings are invalid. Please check configuration.");
                return false;
            }
            
            Debug.Log($"Starting deploy to {deploySettings.ServerHost}:{deploySettings.ServerPort}");
            
            try
            {
                switch (deploySettings.Method)
                {
                    case DeployMethod.FTP:
                        return await DeployViaFTP(deploySettings, buildPath);
                    case DeployMethod.SFTP:
                        return await DeployViaSFTP(deploySettings, buildPath);
                    default:
                        Debug.LogError($"Unsupported deploy method: {deploySettings.Method}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deploy failed: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> DeployViaFTP(DeploySettings settings, string buildPath)
        {
            Debug.Log("FTP Deploy implementation pending...");
            
            // TODO: Реализация FTP деплоя
            // Будет использовать System.Net.FtpWebRequest или внешнюю библиотеку
            
            await Task.Delay(1000); // Имитация работы
            Debug.Log("FTP Deploy completed (mock)");
            return true;
        }
        
        private static async Task<bool> DeployViaSFTP(DeploySettings settings, string buildPath)
        {
            Debug.Log("SFTP Deploy implementation pending...");
            
            // TODO: Реализация SFTP деплоя  
            // Будет использовать SSH.NET или аналогичную библиотеку
            
            await Task.Delay(1000); // Имитация работы
            Debug.Log("SFTP Deploy completed (mock)");
            return true;
        }
        
        private static void ValidateDeployPath(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new DirectoryNotFoundException($"Build path not found: {path}");
            }
        }
        
        private static string[] GetFilesToDeploy(DeploySettings settings, string buildPath)
        {
            if (settings.DeployZipOnly)
            {
                // Ищем ZIP файлы
                var zipFiles = Directory.GetFiles(buildPath, "*.zip", SearchOption.TopDirectoryOnly);
                if (zipFiles.Length == 0)
                {
                    throw new FileNotFoundException("No ZIP files found for deploy");
                }
                return zipFiles;
            }
            else
            {
                // Получаем все файлы и папки
                var files = Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories);
                return files;
            }
        }
    }
}
