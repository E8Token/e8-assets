using System;
using UnityEngine;
using Energy8.JSPluginTools.Communication;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// MonoBehaviour wrapper for DOMService to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/DOM/DOM Service")]
    public class DOMServiceBehaviour : MonoBehaviour
    {
        private IDOMService _service;
        
        /// <summary>
        /// The wrapped DOM service instance
        /// </summary>
        public IDOMService Service => _service;
        
        /// <summary>
        /// Initializes the behaviour with a service instance
        /// </summary>
        /// <param name="service">DOM service to wrap</param>
        public void Initialize(IDOMService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Debug.Log($"[JSPluginTools] DOMServiceBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with the given service
        /// </summary>
        /// <param name="service">DOM service to wrap</param>
        /// <param name="gameObjectName">Name for the GameObject (optional)</param>
        /// <returns>The created behaviour instance</returns>
        public static DOMServiceBehaviour CreateInstance(
            IDOMService service, 
            string gameObjectName = "JSPluginTools_DOMService")
        {
            var existingInstance = FindObjectOfType<DOMServiceBehaviour>();
            if (existingInstance != null)
            {
                existingInstance.Initialize(service);
                return existingInstance;
            }
            
            var gameObject = new GameObject(gameObjectName);
            DontDestroyOnLoad(gameObject);
            var instance = gameObject.AddComponent<DOMServiceBehaviour>();
            instance.Initialize(service);
            
            return instance;
        }
    }
}