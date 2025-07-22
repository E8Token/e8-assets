using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;
using System.IO;

namespace Energy8.BuildDeploySystem.Editor
{
    /// <summary>
    /// Расширенные методы для SSH.NET интеграции
    /// </summary>
    public static class SshExtensions
    {
        /// <summary>
        /// Создает все промежуточные директории на SFTP сервере
        /// </summary>
        public static void CreateDirectoryRecursive(this SftpClient sftpClient, string path)
        {
            if (string.IsNullOrEmpty(path) || sftpClient.Exists(path))
                return;

            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = path.StartsWith("/") ? "/" : "";

            foreach (var part in pathParts)
            {
                currentPath = currentPath.TrimEnd('/') + "/" + part;
                
                if (!sftpClient.Exists(currentPath))
                {
                    sftpClient.CreateDirectory(currentPath);
                }
            }
        }

        /// <summary>
        /// Загружает файл с созданием промежуточных директорий
        /// </summary>
        public static void UploadFileWithDirectories(this SftpClient sftpClient, string localFilePath, string remotePath)
        {
            var remoteDir = Path.GetDirectoryName(remotePath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(remoteDir))
            {
                sftpClient.CreateDirectoryRecursive(remoteDir);
            }

            using var fileStream = File.OpenRead(localFilePath);
            sftpClient.UploadFile(fileStream, remotePath, true);
        }
    }

    /// <summary>
    /// Настройки подключения для различных протоколов
    /// </summary>
    public static class DeployConnectionFactory
    {
        /// <summary>
        /// Создает настроенный FTP клиент с поддержкой FTPS
        /// </summary>
        public static FluentFTP.AsyncFtpClient CreateFtpClient(DeploySettings settings)
        {
            var client = new FluentFTP.AsyncFtpClient(settings.ServerHost, settings.Username, settings.Password, settings.ServerPort);

            // Базовые настройки
            client.Config.ConnectTimeout = 30000;
            client.Config.ReadTimeout = 30000;
            client.Config.DataConnectionType = FluentFTP.FtpDataConnectionType.AutoPassive;

            // Определяем тип шифрования по порту или настройкам
            if (settings.ServerPort == 990 || settings.ServerHost.Contains("ftps"))
            {
                // FTPS (FTP over SSL)
                client.Config.EncryptionMode = FluentFTP.FtpEncryptionMode.Implicit;
                client.Config.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                UnityEngine.Debug.Log("[DeployManager] Using FTPS (implicit SSL) connection");
            }
            else if (settings.ServerPort == 21)
            {
                // Стандартный FTP или FTPS explicit
                client.Config.EncryptionMode = FluentFTP.FtpEncryptionMode.Auto; // Попробует FTPS, если поддерживается
                UnityEngine.Debug.Log("[DeployManager] Using FTP connection (will try FTPS if available)");
            }
            else
            {
                // Обычный FTP
                client.Config.EncryptionMode = FluentFTP.FtpEncryptionMode.None;
                UnityEngine.Debug.Log("[DeployManager] Using plain FTP connection");
            }

            // Настройки для лучшей совместимости
            client.Config.ValidateAnyCertificate = true; // Для self-signed сертификатов
            client.Config.TransferChunkSize = 4096;

            return client;
        }

        /// <summary>
        /// Создает SSH connection info с расширенными настройками
        /// </summary>
        public static ConnectionInfo CreateSshConnection(DeploySettings settings)
        {
            var authMethods = new List<Renci.SshNet.AuthenticationMethod>();

            if (settings.AuthMethod == AuthenticationMethod.PrivateKey && !string.IsNullOrEmpty(settings.PrivateKeyPath))
            {
                try
                {
                    var keyFile = new PrivateKeyFile(settings.PrivateKeyPath);
                    authMethods.Add(new PrivateKeyAuthenticationMethod(settings.Username, keyFile));
                    UnityEngine.Debug.Log($"[DeployManager] Using private key: {settings.PrivateKeyPath}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[DeployManager] Failed to load private key: {ex.Message}");
                    // Fallback to password
                    authMethods.Add(new PasswordAuthenticationMethod(settings.Username, settings.Password));
                }
            }
            else
            {
                authMethods.Add(new PasswordAuthenticationMethod(settings.Username, settings.Password));
                UnityEngine.Debug.Log($"[DeployManager] Using password authentication");
            }

            var connectionInfo = new ConnectionInfo(settings.ServerHost, settings.ServerPort, settings.Username, authMethods.ToArray())
            {
                Timeout = TimeSpan.FromSeconds(30),
            };

            return connectionInfo;
        }
    }
}
