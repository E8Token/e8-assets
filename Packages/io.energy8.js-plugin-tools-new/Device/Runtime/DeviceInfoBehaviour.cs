using System;
using UnityEngine;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.Device
{
    /// <summary>
    /// MonoBehaviour wrapper for DeviceInfo to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/Device/Device Info")]
    public class DeviceInfoBehaviour : MonoBehaviour
    {
        private IDeviceInfo _service;
        
        /// <summary>
        /// The wrapped device info service instance
        /// </summary>
        public IDeviceInfo Service => _service;
        
        /// <summary>
        /// Initializes the behaviour with a service instance
        /// </summary>
        /// <param name="service">Device info service to wrap</param>
        public void Initialize(IDeviceInfo service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Debug.Log($"[JSPluginTools] DeviceInfoBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with the given service
        /// </summary>
        /// <param name="service">Device info service to wrap</param>
        /// <param name="gameObjectName">Name for the GameObject (optional)</param>
        /// <returns>The created behaviour instance</returns>
        public static DeviceInfoBehaviour CreateInstance(
            IDeviceInfo service, 
            string gameObjectName = "JSPluginTools_DeviceInfo")
        {
            var existingInstance = FindObjectOfType<DeviceInfoBehaviour>();
            if (existingInstance != null)
            {
                existingInstance.Initialize(service);
                
                // Также обновим имя объекта, если оно отличается
                if (existingInstance.gameObject.name != gameObjectName)
                {
                    existingInstance.gameObject.name = gameObjectName;
                }
                
                // Применим DontDestroyOnLoad к существующему экземпляру, если он еще не в DontDestroyOnLoad сцене
                if (existingInstance.gameObject.scene.buildIndex != -1)
                {
                    DontDestroyOnLoad(existingInstance.gameObject);
                }
                
                return existingInstance;
            }
            
            var gameObject = new GameObject(gameObjectName);
            DontDestroyOnLoad(gameObject);
            var instance = gameObject.AddComponent<DeviceInfoBehaviour>();
            instance.Initialize(service);
            
            return instance;
        }
    }
}