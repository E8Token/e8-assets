using System;
using System.Collections.Generic;
using UnityEngine;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.Network;
using Energy8.JSPluginTools.DOM;
using Energy8.JSPluginTools.Device;
using Energy8.JSPluginTools.Storage;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Централизованный менеджер для модулей и сервисов JSPluginTools
    /// </summary>
    public class JSPluginToolsManager : MonoBehaviour
    {
        private static JSPluginToolsManager _instance;
        
        /// <summary>
        /// Событие, вызываемое при инициализации всех модулей
        /// </summary>
        public event Action OnAllModulesInitialized;
        
        /// <summary>
        /// Singleton экземпляр менеджера
        /// </summary>
        public static JSPluginToolsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var existing = FindObjectOfType<JSPluginToolsManager>();
                    if (existing != null)
                    {
                        _instance = existing;
                    }
                    else
                    {
                        var go = new GameObject("JSPluginTools_Manager");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<JSPluginToolsManager>();
                    }
                }
                return _instance;
            }
        }
        
        private IPluginCore _pluginCore;
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, IModuleManager> _modules = new Dictionary<Type, IModuleManager>();
        private bool _isInitialized = false;
        
        /// <summary>
        /// Проверяет, инициализирован ли менеджер
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Возвращает ссылку на ядро плагина
        /// </summary>
        public IPluginCore Core => _pluginCore;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                ShutdownAllModules();
            }
        }
        
        /// <summary>
        /// Инициализирует менеджер JSPluginTools
        /// </summary>
        /// <param name="pluginCore">Экземпляр ядра плагина</param>
        public void Initialize(IPluginCore pluginCore)
        {
            if (_isInitialized)
            {
                ErrorHandler.LogWarning("JSPluginToolsManager", "Already initialized");
                return;
            }
            
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "Initialize", () =>
            {
                _pluginCore = pluginCore ?? throw new ArgumentNullException(nameof(pluginCore));
                _isInitialized = true;
                ErrorHandler.LogInfo("JSPluginToolsManager", "Manager initialized");
            });
        }
        
        /// <summary>
        /// Завершает работу менеджера и всех модулей
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                ErrorHandler.LogWarning("JSPluginToolsManager", "Not initialized");
                return;
            }
            
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "Shutdown", () =>
            {
                ShutdownAllModules();
                _isInitialized = false;
                ErrorHandler.LogInfo("JSPluginToolsManager", "Manager shutdown");
            });
        }
        
        /// <summary>
        /// Завершает работу всех модулей
        /// </summary>
        private void ShutdownAllModules()
        {
            foreach (var module in _modules.Values)
            {
                try
                {
                    if (module.IsInitialized)
                    {
                        module.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleError("JSPluginToolsManager", "ShutdownAllModules", ex, false);
                }
            }
            
            _modules.Clear();
        }
        
        /// <summary>
        /// Регистрирует и инициализирует модуль
        /// </summary>
        /// <typeparam name="TInterface">Тип интерфейса модуля</typeparam>
        /// <param name="module">Экземпляр модуля</param>
        /// <exception cref="ArgumentNullException">Если модуль равен null</exception>
        /// <exception cref="InvalidOperationException">Если менеджер не инициализирован</exception>
        public void RegisterModule<TInterface>(IModuleManager module) where TInterface : class, IModuleManager
        {
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "RegisterModule", () =>
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("JSPluginToolsManager is not initialized");
                }
                
                if (module == null)
                {
                    throw new ArgumentNullException(nameof(module));
                }
                
                var interfaceType = typeof(TInterface);
                
                if (_modules.ContainsKey(interfaceType))
                {
                    ErrorHandler.LogWarning("JSPluginToolsManager", $"Module of type {interfaceType.Name} is already registered");
                    return;
                }
                
                _modules[interfaceType] = module;
                
                if (!module.IsInitialized)
                {
                    module.Initialize(_pluginCore);
                }
                
                ErrorHandler.LogInfo("JSPluginToolsManager", $"Registered module {interfaceType.Name}");
            });
        }
        
        /// <summary>
        /// Регистрирует и инициализирует модуль
        /// </summary>
        /// <param name="moduleType">Тип интерфейса модуля</param>
        /// <param name="module">Экземпляр модуля</param>
        /// <exception cref="ArgumentNullException">Если модуль равен null</exception>
        /// <exception cref="InvalidOperationException">Если менеджер не инициализирован</exception>
        public void RegisterModule(Type moduleType, IModuleManager module)
        {
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "RegisterModule", () =>
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("JSPluginToolsManager is not initialized");
                }
                
                if (module == null)
                {
                    throw new ArgumentNullException(nameof(module));
                }
                
                if (_modules.ContainsKey(moduleType))
                {
                    ErrorHandler.LogWarning("JSPluginToolsManager", $"Module of type {moduleType.Name} is already registered");
                    return;
                }
                
                _modules[moduleType] = module;
                
                if (!module.IsInitialized)
                {
                    module.Initialize(_pluginCore);
                }
                
                ErrorHandler.LogInfo("JSPluginToolsManager", $"Registered module {moduleType.Name}");
            });
        }
        
        /// <summary>
        /// Возвращает зарегистрированный модуль по его интерфейсу
        /// </summary>
        /// <typeparam name="T">Тип интерфейса модуля</typeparam>
        /// <returns>Экземпляр модуля или null, если модуль не зарегистрирован</returns>
        public T GetModule<T>() where T : class, IModuleManager
        {
            return ErrorHandler.ExecuteSafe("JSPluginToolsManager", "GetModule", () =>
            {
                if (_modules.TryGetValue(typeof(T), out var module))
                {
                    return module as T;
                }
                
                ErrorHandler.LogWarning("JSPluginToolsManager", $"Module of type {typeof(T).Name} is not registered");
                return null;
            }, (T)null, false);
        }
        
        /// <summary>
        /// Регистрирует сервис и создает соответствующий MonoBehaviour-враппер
        /// </summary>
        /// <typeparam name="TService">Тип интерфейса сервиса</typeparam>
        /// <typeparam name="TBehaviour">Тип MonoBehaviour-враппера</typeparam>
        /// <param name="service">Экземпляр сервиса</param>
        public void RegisterService<TService, TBehaviour>(TService service)
            where TService : class
            where TBehaviour : MonoBehaviour
        {
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "RegisterService", () =>
            {
                if (service == null) throw new ArgumentNullException(nameof(service));
                
                _services[typeof(TService)] = service;
                
                // Вызываем статический метод CreateInstance через рефлексию
                var createMethod = typeof(TBehaviour).GetMethod("CreateInstance", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                if (createMethod != null)
                {
                    createMethod.Invoke(null, new object[] { service });
                    ErrorHandler.LogInfo("JSPluginToolsManager", $"Registered service {typeof(TService).Name} with behaviour {typeof(TBehaviour).Name}");
                }
                else
                {
                    ErrorHandler.LogWarning("JSPluginToolsManager", $"Service behaviour {typeof(TBehaviour).Name} doesn't have a CreateInstance method");
                }
            });
        }
        
        /// <summary>
        /// Возвращает зарегистрированный сервис по его интерфейсу
        /// </summary>
        /// <typeparam name="T">Тип интерфейса сервиса</typeparam>
        /// <returns>Экземпляр сервиса или null, если сервис не зарегистрирован</returns>
        public T GetService<T>() where T : class
        {
            return ErrorHandler.ExecuteSafe("JSPluginToolsManager", "GetService", () =>
            {
                if (_services.TryGetValue(typeof(T), out var service))
                {
                    return service as T;
                }
                
                ErrorHandler.LogWarning("JSPluginToolsManager", $"Service of type {typeof(T).Name} is not registered");
                return null;
            }, (T)null, false);
        }
        
        /// <summary>
        /// Инициализирует все стандартные модули и сервисы JSPluginTools
        /// </summary>
        /// <param name="pluginCore">Экземпляр ядра плагина</param>
        public void InitializeAllModules(IPluginCore pluginCore)
        {
            ErrorHandler.ExecuteSafe("JSPluginToolsManager", "InitializeAllModules", () =>
            {
                Initialize(pluginCore);
                
                // Создаем и регистрируем базовые модули
                var communicationManager = new CommunicationManager();
                RegisterModule<ICommunicationManager>(communicationManager);
                
                var domManager = new DOMManager();
                RegisterModule<IDOMManager>(domManager);
                
                var networkManager = new NetworkManager();
                RegisterModule<INetworkManager>(networkManager);
                
                var storageManager = new StorageManager();
                RegisterModule<IStorageManager>(storageManager);
                
                // Создаем базовые сервисы, которые используют модули
                var communicationService = new CommunicationService((pluginCore as PluginCore)?.MessageBus);
                RegisterService<ICommunicationService, CommunicationServiceBehaviour>(communicationService);
                
                var deviceInfo = new DeviceInfo(communicationService);
                
                // Регистрируем deviceInfo как сервис (он не является модулем)
                _services[typeof(IDeviceInfo)] = deviceInfo;
                
                // Создаем и регистрируем MonoBehaviour для DeviceInfo
                DeviceInfoBehaviour.CreateInstance(deviceInfo);
                
                ErrorHandler.LogInfo("JSPluginToolsManager", "All modules and services initialized");
                
                // Вызываем событие инициализации всех модулей
                OnAllModulesInitialized?.Invoke();
            });
        }
    }
}