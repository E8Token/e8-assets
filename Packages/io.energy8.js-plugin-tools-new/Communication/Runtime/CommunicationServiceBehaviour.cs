using System;
using UnityEngine;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// MonoBehaviour wrapper for CommunicationService to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/Communication/Communication Service")]
    public class CommunicationServiceBehaviour : MonoBehaviour
    {
        private ICommunicationService _service;
        
        /// <summary>
        /// The wrapped communication service instance
        /// </summary>
        public ICommunicationService Service => _service;
        
        /// <summary>
        /// Initializes the behaviour with a service instance
        /// </summary>
        /// <param name="service">Communication service to wrap</param>
        public void Initialize(ICommunicationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Debug.Log($"[JSPluginTools] CommunicationServiceBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with the given service
        /// </summary>
        /// <param name="service">Communication service to wrap</param>
        /// <param name="gameObjectName">Name for the GameObject (optional)</param>
        /// <returns>The created behaviour instance</returns>
        public static CommunicationServiceBehaviour CreateInstance(
            ICommunicationService service, 
            string gameObjectName = "JSPluginTools_CommunicationService")
        {
            var existingInstance = FindObjectOfType<CommunicationServiceBehaviour>();
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
            var instance = gameObject.AddComponent<CommunicationServiceBehaviour>();
            instance.Initialize(service);
            
            return instance;
        }
    }
}