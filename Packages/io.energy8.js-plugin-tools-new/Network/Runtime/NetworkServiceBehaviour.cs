using System;
using UnityEngine;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// MonoBehaviour wrapper for NetworkService to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/Network/Network Service")]
    public class NetworkServiceBehaviour : MonoBehaviour
    {
        private INetworkService _service;
        
        /// <summary>
        /// The wrapped network service instance
        /// </summary>
        public INetworkService Service => _service;
        
        /// <summary>
        /// Initializes the behaviour with a service instance
        /// </summary>
        /// <param name="service">Network service to wrap</param>
        public void Initialize(INetworkService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Debug.Log($"[JSPluginTools] NetworkServiceBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with the given service
        /// </summary>
        /// <param name="service">Network service to wrap</param>
        /// <param name="gameObjectName">Name for the GameObject (optional)</param>
        /// <returns>The created behaviour instance</returns>
        public static NetworkServiceBehaviour CreateInstance(
            INetworkService service, 
            string gameObjectName = "JSPluginTools_NetworkService")
        {
            var existingInstance = FindObjectOfType<NetworkServiceBehaviour>();
            if (existingInstance != null)
            {
                existingInstance.Initialize(service);
                return existingInstance;
            }
            
            var gameObject = new GameObject(gameObjectName);
            DontDestroyOnLoad(gameObject);
            var instance = gameObject.AddComponent<NetworkServiceBehaviour>();
            instance.Initialize(service);
            
            return instance;
        }
    }
}