using System;
using UnityEngine;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.Storage
{
    /// <summary>
    /// MonoBehaviour wrapper for StorageService to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/Storage/Storage Service")]
    public class StorageServiceBehaviour : MonoBehaviour
    {
        private IStorageService _service;
        
        /// <summary>
        /// The wrapped storage service instance
        /// </summary>
        public IStorageService Service => _service;
        
        /// <summary>
        /// Initializes the behaviour with a service instance
        /// </summary>
        /// <param name="service">Storage service to wrap</param>
        public void Initialize(IStorageService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Debug.Log($"[JSPluginTools] StorageServiceBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with the given service
        /// </summary>
        /// <param name="service">Storage service to wrap</param>
        /// <param name="gameObjectName">Name for the GameObject (optional)</param>
        /// <returns>The created behaviour instance</returns>
        public static StorageServiceBehaviour CreateInstance(
            IStorageService service, 
            string gameObjectName = "JSPluginTools_StorageService")
        {
            var existingInstance = FindObjectOfType<StorageServiceBehaviour>();
            if (existingInstance != null)
            {
                existingInstance.Initialize(service);
                return existingInstance;
            }
            
            var gameObject = new GameObject(gameObjectName);
            DontDestroyOnLoad(gameObject);
            var instance = gameObject.AddComponent<StorageServiceBehaviour>();
            instance.Initialize(service);
            
            return instance;
        }
    }
}