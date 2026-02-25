using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using FluentFTP;
using System.IO.Compression;
using System.Linq;

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

            try
            {
                bool result;
                switch (deploySettings.Method)
                {
                    case DeployMethod.FTP:
                        result = await DeployViaFTP(deploySettings, buildPath);
                        break;
                    case DeployMethod.SFTP:
                        result = await DeployViaSFTP(deploySettings, buildPath);
                        break;
                    case DeployMethod.LocalCopy:
                        result = await DeployViaLocalCopy(deploySettings, buildPath);
                        break;
                    default:
                        Debug.LogError($"Unsupported deploy method: {deploySettings.Method}");
                        return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deploy failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> DeployViaFTP(DeploySettings settings, string buildPath)
        {
            Debug.Log($"[DeployManager] Starting FTP deploy to {settings.ServerHost}:{settings.ServerPort}");

            try
            {
                EditorUtility.DisplayProgressBar("Deploying via FTP", "Connecting to server...", 0f);

                ValidateDeployPath(buildPath);
                var filesToDeploy = GetFilesToDeploy(settings, buildPath);

                // Создаем FTP клиент с расширенными настройками
                using var ftpClient = DeployConnectionFactory.CreateFtpClient(settings);

                // Подключаемся
                EditorUtility.DisplayProgressBar("Deploying via FTP", "Connecting to server...", 0.1f);
                await ftpClient.Connect();
                Debug.Log($"[DeployManager] Connected to FTP server: {settings.ServerHost} (Encryption: {ftpClient.Config.EncryptionMode})");

                // 1. Создаем удаленную директорию если не существует
                EditorUtility.DisplayProgressBar("Deploying via FTP", "Creating remote directory...", 0.2f);
                if (!await ftpClient.DirectoryExists(settings.RemotePath))
                {
                    await ftpClient.CreateDirectory(settings.RemotePath);
                    Debug.Log($"[DeployManager] Created remote directory: {settings.RemotePath}");
                }

                // 2. Удаляем существующие файлы
                EditorUtility.DisplayProgressBar("Deploying via FTP", "Deleting existing files...", 0.3f);
                try
                {
                    var existingFiles = await ftpClient.GetListing(settings.RemotePath);
                    foreach (var item in existingFiles)
                    {
                        if (item.Type == FluentFTP.FtpObjectType.File)
                        {
                            await ftpClient.DeleteFile(item.FullName);
                            Debug.Log($"[DeployManager] Deleted: {item.Name}");
                        }
                    }
                    Debug.Log("[DeployManager] Existing files deleted");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DeployManager] Could not delete some existing files: {ex.Message}");
                }

                // 4. Загружаем файлы с progress tracking
                int uploadedCount = 0;
                int totalFiles = filesToDeploy.Length;
                long totalBytes = filesToDeploy.Sum(f => new FileInfo(f).Length);
                long uploadedBytes = 0;

                foreach (var filePath in filesToDeploy)
                {
                    var relativePath = Path.GetRelativePath(buildPath, filePath);
                    var remotePath = $"{settings.RemotePath}/{relativePath.Replace('\\', '/')}";

                    // Создаем промежуточные директории
                    var remoteDir = Path.GetDirectoryName(remotePath)?.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(remoteDir) && !await ftpClient.DirectoryExists(remoteDir))
                    {
                        await ftpClient.CreateDirectory(remoteDir);
                    }

                    // Загружаем файл с callback для прогресса
                    var fileInfo = new FileInfo(filePath);
                    
                    float fileProgress = 0f;
                    var progress = new Progress<FtpProgress>(p =>
                    {
                        fileProgress = (float)(p.Progress / 100f);
                        float overallProgress = 0.4f + ((float)(uploadedBytes + (long)(fileInfo.Length * p.Progress / 100.0)) * 100f / (float)totalBytes) * 0.6f / 100f;
                        EditorUtility.DisplayProgressBar("Deploying via FTP", $"Uploading: {relativePath} ({p.Progress}%)", overallProgress);
                    });

                    var result = await ftpClient.UploadFile(filePath, remotePath, FtpRemoteExists.Overwrite, false, progress: progress);

                    if (result == FtpStatus.Success)
                    {
                        uploadedCount++;
                        uploadedBytes += fileInfo.Length;
                        Debug.Log($"[DeployManager] Uploaded ({uploadedCount}/{totalFiles}): {relativePath} ({fileInfo.Length / 1024} KB)");
                    }
                    else
                    {
                        Debug.LogWarning($"[DeployManager] Failed to upload: {relativePath} (Status: {result})");
                    }

                    await Task.Yield();
                }

                await ftpClient.Disconnect();
                EditorUtility.ClearProgressBar();
                Debug.Log($"[DeployManager] FTP deployment completed! Files: {uploadedCount}/{totalFiles}, Size: {uploadedBytes / 1024} KB");
                return uploadedCount == totalFiles;
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[DeployManager] FTP deploy failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> DeployViaSFTP(DeploySettings settings, string buildPath)
        {
            Debug.Log($"[DeployManager] Starting SFTP deploy to {settings.ServerHost}:{settings.ServerPort}");

            try
            {
                EditorUtility.DisplayProgressBar("Deploying via SFTP", "Connecting to server...", 0f);

                ValidateDeployPath(buildPath);
                var filesToDeploy = GetFilesToDeploy(settings, buildPath);

                // Создаем архив если много файлов
                string tempArchivePath = null;
                bool useArchive = filesToDeploy.Length > 50;

                if (useArchive)
                {
                    EditorUtility.DisplayProgressBar("Deploying via SFTP", "Creating archive...", 0.1f);
                    tempArchivePath = await CreateZipArchive(buildPath);
                    filesToDeploy = new[] { tempArchivePath };
                    Debug.Log($"[DeployManager] Using archive mode: {Path.GetFileName(tempArchivePath)}");
                }

                try
                {
                    // Создаем SSH подключение с расширенными настройками
                    ConnectionInfo connectionInfo = DeployConnectionFactory.CreateSshConnection(settings);

                    using var sshClient = new SshClient(connectionInfo);
                    using var sftpClient = new SftpClient(connectionInfo);

                    // Подключаемся
                    EditorUtility.DisplayProgressBar("Deploying via SFTP", "Connecting to server...", 0.2f);
                    await Task.Run(() =>
                    {
                        sshClient.Connect();
                        sftpClient.Connect();
                    });

                    Debug.Log($"[DeployManager] Connected to SFTP server: {settings.ServerHost}");

                    // Создаем удаленную директорию рекурсивно
                    sftpClient.CreateDirectoryRecursive(settings.RemotePath);

                    // 1. Создаем бекап если нужно
                    EditorUtility.DisplayProgressBar("Deploying via SFTP", "Creating backup...", 0.3f);
                    if (!await CreateBackupIfNeeded(settings, settings.RemotePath, sshClient))
                    {
                        Debug.LogError("[DeployManager] Backup creation failed, aborting deploy");
                        return false;
                    }

                    // 2. Удаляем существующие файлы если нужно
                    EditorUtility.DisplayProgressBar("Deploying via SFTP", "Deleting existing files...", 0.4f);
                    if (!await DeleteExistingFilesIfNeeded(settings, settings.RemotePath, sshClient))
                    {
                        Debug.LogError("[DeployManager] Failed to delete existing files, aborting deploy");
                        return false;
                    }

                    // 3. Загружаем файлы с прогрессом
                    int uploadedCount = 0;
                    int totalFiles = filesToDeploy.Length;
                    long totalBytes = filesToDeploy.Sum(f => new FileInfo(f).Length);
                    long uploadedBytes = 0;

                    foreach (var filePath in filesToDeploy)
                    {
                        var fileName = Path.GetFileName(filePath);
                        var fileInfo = new FileInfo(filePath);

                        float overallProgress = 0.5f + ((float)(uploadedBytes * 100.0 / (double)totalBytes) * 0.4f / 100f);
                        EditorUtility.DisplayProgressBar("Deploying via SFTP", $"Uploading: {fileName} ({uploadedCount + 1}/{totalFiles})", overallProgress);

                        string remotePath;
                        if (useArchive)
                        {
                            remotePath = $"{settings.RemotePath.TrimEnd('/')}/{fileName}";
                        }
                        else
                        {
                            var relativePath = Path.GetRelativePath(buildPath, filePath);
                            remotePath = $"{settings.RemotePath.TrimEnd('/')}/{relativePath.Replace('\\', '/')}";
                        }

                        // Загружаем файл с созданием директорий
                        await Task.Run(() => sftpClient.UploadFileWithDirectories(filePath, remotePath));

                        uploadedCount++;
                        uploadedBytes += fileInfo.Length;
                        Debug.Log($"[DeployManager] Uploaded ({uploadedCount}/{totalFiles}): {fileName} ({fileInfo.Length / 1024} KB)");

                        if (useArchive && filePath == tempArchivePath)
                        {
                            EditorUtility.DisplayProgressBar("Deploying via SFTP", "Extracting archive on server...", 0.9f);
                            await ExtractArchiveOnServer(sshClient, settings.RemotePath, fileName);
                        }

                        await Task.Yield();
                    }

                    sftpClient.Disconnect();
                    sshClient.Disconnect();

                    EditorUtility.ClearProgressBar();
                    Debug.Log($"[DeployManager] SFTP deployment completed! Files: {uploadedCount}/{totalFiles}, Size: {uploadedBytes / 1024} KB");
                    return uploadedCount == totalFiles;
                }
                finally
                {
                    // Удаляем временный архив
                    if (tempArchivePath != null && File.Exists(tempArchivePath))
                    {
                        File.Delete(tempArchivePath);
                        Debug.Log($"[DeployManager] Cleaned up temporary archive: {Path.GetFileName(tempArchivePath)}");
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[DeployManager] SFTP deploy failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Создает ZIP архив вместо tar.gz для кроссплатформенности
        /// </summary>
        private static async Task<string> CreateZipArchive(string sourcePath)
        {
            string tempPath = Path.GetTempPath();
            string archiveName = $"build_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            string archivePath = Path.Combine(tempPath, archiveName);

            Debug.Log($"[DeployManager] Creating ZIP archive: {archiveName}");

            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(sourcePath, archivePath, System.IO.Compression.CompressionLevel.Optimal, false);
            });

            var fileInfo = new FileInfo(archivePath);
            Debug.Log($"[DeployManager] Archive created: {fileInfo.Length / 1024} KB");

            return archivePath;
        }

        /// <summary>
        /// Создает бекап директории (для LocalCopy) или команду бекапа для удаленного сервера
        /// </summary>
        private static async Task<bool> CreateBackupIfNeeded(DeploySettings settings, string targetPath, SshClient sshClient = null)
        {
            try
            {
                if (settings.Method == DeployMethod.LocalCopy)
                {
                    return await CreateLocalBackup(targetPath);
                }
                else if (sshClient != null)
                {
                    return await CreateRemoteBackup(sshClient, settings.RemotePath);
                }
                
                Debug.LogWarning("[DeployManager] Backup requested but no valid method available");
                return true; // Не блокируем deploy если бекап не критичен
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] Backup creation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Создает локальный бекап директории
        /// </summary>
        private static async Task<bool> CreateLocalBackup(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Debug.Log("[DeployManager] Target directory doesn't exist, skipping backup");
                return true;
            }

            string backupPath = $"{targetPath}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            Debug.Log($"[DeployManager] Creating local backup: {backupPath}");

            await Task.Run(() =>
            {
                DirectoryCopy(targetPath, backupPath, true);
            });

            Debug.Log($"[DeployManager] Local backup created successfully: {backupPath}");
            return true;
        }

        /// <summary>
        /// Создает бекап (максимум 1 бекап) и заменяет целевую директорию временной
        /// </summary>
        private static async Task<bool> CreateBackupAndSwapLocal(string targetPath, string tempPath)
        {
            string parentDir = Path.GetDirectoryName(targetPath);
            string targetDirName = Path.GetFileName(targetPath);

            try
            {
                // 1. Находим и удаляем старый бекап если есть
                string backupPattern = $"{targetPath}_backup_*";
                var existingBackups = Directory.GetDirectories(parentDir, $"{targetDirName}_backup_*");
                
                foreach (var oldBackup in existingBackups)
                {
                    Debug.Log($"[DeployManager] Deleting old backup: {oldBackup}");
                    await Task.Run(() => Directory.Delete(oldBackup, true));
                }

                // 2. Создаем бекап если целевая директория существует
                if (Directory.Exists(targetPath))
                {
                    string newBackupPath = $"{targetPath}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    Debug.Log($"[DeployManager] Creating backup: {newBackupPath}");
                    await Task.Run(() => Directory.Move(targetPath, newBackupPath));
                }

                // 3. Переименовываем временную директорию в целевую
                Debug.Log($"[DeployManager] Renaming temp to target: {tempPath} -> {targetPath}");
                await Task.Run(() => Directory.Move(tempPath, targetPath));

                Debug.Log($"[DeployManager] Backup and swap completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] CreateBackupAndSwapLocal failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Создает удаленный бекап через SSH команды (максимум 1 бекап)
        /// </summary>
        private static async Task<bool> CreateRemoteBackup(SshClient sshClient, string remotePath)
        {
            try
            {
                string backupPath = $"{remotePath}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                
                // Сначала удаляем старые бекапы по паттерну
                string cleanupCommand = $"for dir in $(ls -d \"{remotePath}_backup_\"* 2>/dev/null); do rm -rf \"$dir\" && echo \"Deleted old backup: $dir\"; done";
                
                Debug.Log($"[DeployManager] Cleaning up old remote backups...");
                var cleanupCmd = sshClient.CreateCommand(cleanupCommand);
                await Task.Run(() => cleanupCmd.Execute());
                
                // Создаем новый бекап
                string backupCommand = $"if [ -d \"{remotePath}\" ]; then cp -r \"{remotePath}\" \"{backupPath}\"; echo \"Backup created: {backupPath}\"; else echo \"Directory doesn't exist, skipping backup\"; fi";

                Debug.Log($"[DeployManager] Creating remote backup: {backupPath}");

                var command = sshClient.CreateCommand(backupCommand);
                await Task.Run(() => command.Execute());

                if (command.ExitStatus == 0)
                {
                    Debug.Log($"[DeployManager] Remote backup created: {command.Result.Trim()}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[DeployManager] Backup command warning: {command.Error}");
                    return true; // Не блокируем deploy если бекап не критичен
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] Remote backup failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Удаляет существующие файлы (всегда выполняется)
        /// </summary>
        private static async Task<bool> DeleteExistingFilesIfNeeded(DeploySettings settings, string targetPath, SshClient sshClient = null)
        {
            try
            {
                if (settings.Method == DeployMethod.LocalCopy)
                {
                    return await DeleteLocalDirectory(targetPath);
                }
                else if (sshClient != null)
                {
                    return await DeleteRemoteDirectory(sshClient, settings.RemotePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] Failed to delete existing files: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Удаляет локальную директорию
        /// </summary>
        private static async Task<bool> DeleteLocalDirectory(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Debug.Log("[DeployManager] Target directory doesn't exist, nothing to delete");
                return true;
            }

            Debug.Log($"[DeployManager] Deleting existing directory: {targetPath}");

            await Task.Run(() =>
            {
                Directory.Delete(targetPath, true);
            });

            Debug.Log("[DeployManager] Existing directory deleted successfully");
            return true;
        }

        /// <summary>
        /// Удаляет удаленную директорию через SSH
        /// </summary>
        private static async Task<bool> DeleteRemoteDirectory(SshClient sshClient, string remotePath)
        {
            try
            {
                string deleteCommand = $"if [ -d \"{remotePath}\" ]; then rm -rf \"{remotePath}\"/*; echo \"Directory contents deleted\"; else echo \"Directory doesn't exist\"; fi";

                Debug.Log($"[DeployManager] Deleting remote directory contents: {remotePath}");

                var command = sshClient.CreateCommand(deleteCommand);
                var result = await Task.Run(() => command.Execute());

                if (command.ExitStatus == 0)
                {
                    Debug.Log($"[DeployManager] Remote directory cleared: {command.Result.Trim()}");
                    return true;
                }
                else
                {
                    Debug.LogError($"[DeployManager] Failed to delete remote directory: {command.Error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] Remote delete failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Рекурсивно копирует директорию (для бекапа)
        /// </summary>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        private static async Task<bool> ExtractArchiveOnServer(SshClient sshClient, string remotePath, string archiveFileName)
        {
            try
            {
                Debug.Log($"[DeployManager] Extracting archive on server: {archiveFileName}");

                string extractCommand;
                if (archiveFileName.EndsWith(".zip"))
                {
                    // Команда для ZIP
                    extractCommand = $"cd {remotePath} && unzip -o {archiveFileName} && rm {archiveFileName}";
                }
                else
                {
                    // Команда для tar.gz (на случай если останется)
                    extractCommand = $"cd {remotePath} && tar -xzf {archiveFileName} && rm {archiveFileName}";
                }

                var command = sshClient.CreateCommand(extractCommand);
                var result = await Task.Run(() => command.Execute());

                if (command.ExitStatus == 0)
                {
                    Debug.Log("[DeployManager] Archive extracted successfully!");
                    return true;
                }
                else
                {
                    Debug.LogError($"[DeployManager] Archive extraction failed: {command.Error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployManager] Archive extraction error: {ex.Message}");
                return false;
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
            var files = Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories);
            return files;
        }

        private static async Task<bool> DeployViaLocalCopy(DeploySettings settings, string buildPath)
        {
            try
            {
                Debug.Log("[DeployManager] Starting local copy deploy...");

                if (string.IsNullOrWhiteSpace(settings.LocalCopyTargetPath))
                {
                    Debug.LogError("[DeployManager] LocalCopyTargetPath is not set.");
                    return false;
                }
                if (!Directory.Exists(buildPath))
                {
                    Debug.LogError($"[DeployManager] Build path does not exist: {buildPath}");
                    return false;
                }
                string targetPath = settings.LocalCopyTargetPath;
                string parentDir = Path.GetDirectoryName(targetPath);
                string targetDirName = Path.GetFileName(targetPath);
                string tempPath = Path.Combine(parentDir, $"{targetDirName}_temp_{DateTime.Now:yyyyMMdd_HHmmss}");

                try
                {
                    // 1. Копируем файлы во временную директорию
                    EditorUtility.DisplayProgressBar("Local Copy Deploy", "Copying to temp directory...", 0.1f);
                    DirectoryCopy(buildPath, tempPath, true);
                    Debug.Log($"[DeployManager] Copied build to temp directory: {tempPath}");

                    // 2. Создаем бекап и переименовываем
                    EditorUtility.DisplayProgressBar("Local Copy Deploy", "Creating backup and swapping directories...", 0.8f);
                    if (!await CreateBackupAndSwapLocal(targetPath, tempPath))
                    {
                        Debug.LogError("[DeployManager] Failed to create backup and swap, aborting deploy");
                        return false;
                    }

                    EditorUtility.ClearProgressBar();
                    Debug.Log($"[DeployManager] Local copy deploy completed successfully");
                    return true;
                }
                catch
                {
                    // Удаляем временную директорию при ошибке
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                        Debug.Log($"[DeployManager] Cleaned up temp directory: {tempPath}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[DeployManager] Local copy deploy failed: {ex.Message}");
                return false;
            }
        }
    }
}
