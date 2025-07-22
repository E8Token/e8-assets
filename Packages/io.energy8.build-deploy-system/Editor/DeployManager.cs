using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Net;
using Debug = UnityEngine.Debug;

namespace Energy8.BuildDeploySystem.Editor
{    public interface IDeployMonitor
    {
        void AddLog(string message, UnityEngine.LogType type = UnityEngine.LogType.Log);
        void UpdateDeployProgress(float progress, string message = null);
        void CompleteDeployment(bool success);
    }
    
    public class ConsoleDeployMonitor : IDeployMonitor
    {
        public void AddLog(string message, UnityEngine.LogType type = UnityEngine.LogType.Log)
        {
            switch (type)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                    Debug.LogError(message);
                    break;
                case UnityEngine.LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
        
        public void UpdateDeployProgress(float progress, string message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log($"[{progress * 100:F0}%] {message}");
            }
        }
        
        public void CompleteDeployment(bool success)
        {
            if (success)
            {
                Debug.Log("✅ [DeploySystem] Deployment completed successfully!");
            }
            else
            {
                Debug.LogError("❌ [DeploySystem] Deployment failed!");
            }
        }
    }
      public static class DeployManager
    {
        public static async Task<bool> DeployBuild(BuildConfiguration config, string buildPath, IDeployMonitor monitor = null)
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
            
            // Используем переданный монитор или создаем консольный
            IDeployMonitor deployMonitor = monitor ?? new ConsoleDeployMonitor();
            deployMonitor.AddLog($"[DeploySystem] Starting deployment...", UnityEngine.LogType.Log);
            
            try
            {
                bool result;
                switch (deploySettings.Method)
                {
                    case Energy8.BuildDeploySystem.DeployMethod.FTP:
                        result = await DeployViaFTP(deploySettings, buildPath, deployMonitor);
                        break;
                    case Energy8.BuildDeploySystem.DeployMethod.SFTP:
                        result = await DeployViaSFTP(deploySettings, buildPath, deployMonitor);
                        break;
                    case Energy8.BuildDeploySystem.DeployMethod.LocalCopy:
                        result = await DeployViaLocalCopy(deploySettings, buildPath, deployMonitor);
                        break;
                    default:
                        Debug.LogError($"Unsupported deploy method: {deploySettings.Method}");
                        deployMonitor.AddLog($"❌ Unsupported deploy method: {deploySettings.Method}", UnityEngine.LogType.Error);
                        deployMonitor.CompleteDeployment(false);
                        return false;
                }
                
                deployMonitor.CompleteDeployment(result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deploy failed: {ex.Message}");
                deployMonitor.AddLog($"❌ Deploy failed: {ex.Message}", UnityEngine.LogType.Error);
                deployMonitor.CompleteDeployment(false);
                return false;
            }
        }        private static async Task<bool> DeployViaFTP(DeploySettings settings, string buildPath, IDeployMonitor monitor)
        {
            Debug.Log($"Starting FTP deploy to {settings.ServerHost}:{settings.ServerPort}");
            monitor.AddLog($"🌐 Starting FTP deploy to {settings.ServerHost}:{settings.ServerPort}");
            
            try
            {
                ValidateDeployPath(buildPath);
                var filesToDeploy = GetFilesToDeploy(settings, buildPath);
                
                monitor.AddLog($"📁 Found {filesToDeploy.Length} files to deploy");
                
                for (int i = 0; i < filesToDeploy.Length; i++)
                {
                    var filePath = filesToDeploy[i];
                    var relativePath = Path.GetRelativePath(buildPath, filePath);
                    
                    monitor.UpdateDeployProgress((float)i / filesToDeploy.Length, $"📤 Uploading: {relativePath}");
                    await UploadFileViaFTP(settings, filePath, buildPath);
                }
                
                Debug.Log("[DeploySystem] FTP Deployment completed successfully!");
                monitor.AddLog("✅ FTP Deployment completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeploySystem] FTP Deploy failed: {ex.Message}");
                monitor.AddLog($"❌ FTP Deploy failed: {ex.Message}", UnityEngine.LogType.Error);
                return false;
            }
        }        private static async Task<bool> DeployViaSFTP(DeploySettings settings, string buildPath, IDeployMonitor monitor)
        {
            Debug.Log($"Starting SFTP deploy to {settings.ServerHost}:{settings.ServerPort}");
            
            try
            {
                ValidateDeployPath(buildPath);
                var filesToDeploy = GetFilesToDeploy(settings, buildPath);
                
                Debug.Log($"[DeploySystem] Found {filesToDeploy.Length} files to deploy");
                Debug.Log($"[DeploySystem] Target: {settings.Username}@{settings.ServerHost}:{settings.RemotePath}");
                Debug.Log($"[DeploySystem] Auth method: {settings.AuthMethod}");
                
                // Создаем временную папку для архива
                string tempDir = Path.Combine(Path.GetTempPath(), "Energy8Deploy_" + System.Guid.NewGuid().ToString("N")[..8]);
                Directory.CreateDirectory(tempDir);
                
                try
                {
                    // Создаем архив билда
                    string archiveName = $"build_{DateTime.Now:yyyyMMdd_HHmmss}.tar.gz";
                    string archivePath = Path.Combine(tempDir, archiveName);
                    
                    await CreateArchive(buildPath, archivePath);
                    
                    // Загружаем через SCP
                    bool uploadSuccess = await UploadViaSCP(settings, archivePath, archiveName);
                    
                    if (uploadSuccess)
                    {
                        // Распаковываем на сервере
                        bool extractSuccess = await ExtractOnServer(settings, archiveName);
                        
                        if (extractSuccess)
                        {
                            Debug.Log("[DeploySystem] SFTP Deployment completed successfully!");
                            return true;
                        }
                    }
                    
                    return false;
                }
                finally
                {
                    // Очищаем временные файлы
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeploySystem] SFTP Deploy failed: {ex.Message}");
                return false;
            }
        }        
        private static async Task CreateArchive(string sourcePath, string archivePath)
        {
            Debug.Log($"[DeploySystem] Creating archive: {Path.GetFileName(archivePath)}");
            
            // Используем tar для создания архива (работает на Windows 10+ и всех Unix системах)
            var processInfo = new ProcessStartInfo
            {
                FileName = "tar",
                Arguments = $"-czf \"{archivePath}\" -C \"{sourcePath}\" .",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
              using (var process = Process.Start(processInfo))
            {
                await Task.Run(() => process.WaitForExit());
                
                if (process.ExitCode != 0)
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"Archive creation failed: {error}");
                }
            }
            
            Debug.Log($"[DeploySystem] Archive created: {new FileInfo(archivePath).Length / 1024} KB");
        }
        
        private static async Task<bool> UploadViaSCP(DeploySettings settings, string localFile, string remoteFileName)
        {
            Debug.Log($"[DeploySystem] Uploading via SCP: {remoteFileName}");
            
            string remotePath = $"{settings.Username}@{settings.ServerHost}:{settings.RemotePath}/{remoteFileName}";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "scp",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            // Настраиваем аргументы в зависимости от метода аутентификации
            if (settings.AuthMethod == AuthenticationMethod.PrivateKey && !string.IsNullOrEmpty(settings.PrivateKeyPath))
            {
                processInfo.Arguments = $"-P {settings.ServerPort} -i \"{settings.PrivateKeyPath}\" \"{localFile}\" {remotePath}";
            }
            else
            {
                processInfo.Arguments = $"-P {settings.ServerPort} \"{localFile}\" {remotePath}";
                // Для password auth нужно будет использовать sshpass или аналог
                Debug.LogWarning("[DeploySystem] Password authentication requires sshpass. Consider using SSH keys.");
            }
              Debug.Log($"[DeploySystem] SCP command: scp {processInfo.Arguments}");
            
            using (var process = Process.Start(processInfo))
            {
                await Task.Run(() => process.WaitForExit());
                
                if (process.ExitCode == 0)
                {
                    Debug.Log("[DeploySystem] Upload completed successfully!");
                    return true;
                }
                else
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    Debug.LogError($"[DeploySystem] SCP upload failed: {error}");
                    return false;
                }
            }
        }
        
        private static async Task<bool> ExtractOnServer(DeploySettings settings, string archiveFileName)
        {
            Debug.Log($"[DeploySystem] Extracting archive on server: {archiveFileName}");
            
            string sshCommand = $"cd {settings.RemotePath} && tar -xzf {archiveFileName} && rm {archiveFileName}";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "ssh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            // Настраиваем SSH команду
            if (settings.AuthMethod == AuthenticationMethod.PrivateKey && !string.IsNullOrEmpty(settings.PrivateKeyPath))
            {
                processInfo.Arguments = $"-p {settings.ServerPort} -i \"{settings.PrivateKeyPath}\" {settings.Username}@{settings.ServerHost} \"{sshCommand}\"";
            }
            else
            {
                processInfo.Arguments = $"-p {settings.ServerPort} {settings.Username}@{settings.ServerHost} \"{sshCommand}\"";
            }
              Debug.Log($"[DeploySystem] SSH command: ssh {processInfo.Arguments}");
            
            using (var process = Process.Start(processInfo))
            {
                await Task.Run(() => process.WaitForExit());
                
                if (process.ExitCode == 0)
                {
                    Debug.Log("[DeploySystem] Archive extracted successfully!");
                    return true;
                }
                else
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    Debug.LogError($"[DeploySystem] SSH extraction failed: {error}");
                    return false;
                }
            }
        }
        
        private static async Task UploadFileViaFTP(DeploySettings settings, string localFilePath, string buildBasePath)
        {
            var relativePath = Path.GetRelativePath(buildBasePath, localFilePath);
            var remoteUrl = $"ftp://{settings.ServerHost}:{settings.ServerPort}{settings.RemotePath}/{relativePath.Replace('\\', '/')}";
            
            var request = (FtpWebRequest)WebRequest.Create(remoteUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(settings.Username, settings.Password);
            request.UseBinary = true;
            request.UsePassive = true;
            
            var fileContents = await File.ReadAllBytesAsync(localFilePath);
            request.ContentLength = fileContents.Length;
            
            using (var requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }
            
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                Debug.Log($"[DeploySystem] Uploaded: {relativePath} ({response.StatusDescription})");
            }
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
        
        /// <summary>
        /// Копирует билд в локальную папку
        /// </summary>
        private static async Task<bool> DeployViaLocalCopy(Energy8.BuildDeploySystem.DeploySettings settings, string buildPath, IDeployMonitor monitor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.LocalCopyTargetPath))
                {
                    monitor.AddLog("LocalCopyTargetPath не указан!", UnityEngine.LogType.Error);
                    return false;
                }
                if (!Directory.Exists(buildPath))
                {
                    monitor.AddLog($"Build path не найден: {buildPath}", UnityEngine.LogType.Error);
                    return false;
                }
                string targetPath = settings.LocalCopyTargetPath;
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                var files = Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories);
                monitor.AddLog($"Копируем {files.Length} файлов в {targetPath}");
                int copied = 0;
                foreach (var file in files)
                {
                    string relPath = Path.GetRelativePath(buildPath, file);
                    string destFile = Path.Combine(targetPath, relPath);
                    string destDir = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    File.Copy(file, destFile, true);
                    copied++;
                    monitor.UpdateDeployProgress((float)copied / files.Length, $"Копируем: {relPath}");
                    await Task.Yield();
                }
                monitor.AddLog($"✅ Локальный деплой завершён: {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                monitor.AddLog($"❌ Ошибка локального деплоя: {ex.Message}", UnityEngine.LogType.Error);
                return false;
            }
        }
    }
}
