using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Storage
{
    /// <summary>
    /// Интерфейс для работы с хранилищем данных в браузере
    /// </summary>
    public interface IStorageManager
    {
        /// <summary>
        /// Инициализирует менеджер хранилища с указанным ядром плагина
        /// </summary>
        /// <param name="core">Экземпляр ядра плагина</param>
        void Initialize(IPluginCore core);
        
        /// <summary>
        /// Проверяет, инициализирован ли модуль
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Событие, возникающее при инициализации модуля
        /// </summary>
        event Action OnInitialized;
        
        /// <summary>
        /// Сохраняет значение в хранилище
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если операция выполнена успешно</returns>
        Task<bool> SetItemAsync(string key, string value, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Сохраняет объект в хранилище в виде JSON
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Объект для сохранения</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если операция выполнена успешно</returns>
        Task<bool> SetJsonItemAsync<T>(string key, T value, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Получает значение из хранилища
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>Значение или null, если ключ не найден</returns>
        Task<string> GetItemAsync(string key, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Получает объект из хранилища, десериализуя его из JSON
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>Объект или default(T), если ключ не найден или произошла ошибка десериализации</returns>
        Task<T> GetJsonItemAsync<T>(string key, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Удаляет элемент из хранилища
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если операция выполнена успешно</returns>
        Task<bool> RemoveItemAsync(string key, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Проверяет наличие ключа в хранилище
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если ключ существует</returns>
        Task<bool> HasKeyAsync(string key, StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Очищает всё хранилище
        /// </summary>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если операция выполнена успешно</returns>
        Task<bool> ClearAsync(StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Получает все ключи из хранилища
        /// </summary>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>Массив ключей</returns>
        Task<string[]> GetKeysAsync(StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Получает размер данных в хранилище (в байтах)
        /// </summary>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>Размер данных в байтах</returns>
        Task<long> GetSizeAsync(StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// Проверяет доступность хранилища в браузере
        /// </summary>
        /// <param name="storageType">Тип хранилища</param>
        /// <returns>True, если хранилище доступно</returns>
        Task<bool> IsAvailableAsync(StorageType storageType = StorageType.Local);
    }
}