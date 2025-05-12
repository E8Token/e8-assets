using System;
using UnityEngine;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// MonoBehaviour wrapper for PluginCore to allow finding it with FindObjectOfType
    /// </summary>
    [AddComponentMenu("JSPluginTools/Core/Plugin Core")]
    public class PluginCoreBehaviour : MonoBehaviour
    {
        private PluginCore _core;
        
        /// <summary>
        /// The wrapped PluginCore instance
        /// </summary>
        public PluginCore Core => _core;
        
        /// <summary>
        /// Initializes the behaviour with a core instance
        /// </summary>
        public void Initialize(PluginCore core)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            Debug.Log($"[JSPluginTools] PluginCoreBehaviour initialized");
        }
        
        /// <summary>
        /// Creates a GameObject with this behaviour and initializes it with a new or provided core
        /// </summary>
        public static PluginCoreBehaviour CreateInstance(
            PluginCore core = null, 
            string gameObjectName = "JSPluginTools_PluginCore")
        {
            var existingInstance = FindAnyObjectByType<PluginCoreBehaviour>();
            if (existingInstance != null)
            {
                if (core != null)
                {
                    existingInstance.Initialize(core);
                }
                return existingInstance;
            }
            
            // Create new core with required dependencies if not provided
            if (core == null)
            {
                var memoryManager = new MemoryManager();
                var messageBus = new MessageBus(memoryManager); // Pass the memoryManager to MessageBus constructor
                core = new PluginCore(memoryManager, messageBus);
            }
            
            var gameObject = new GameObject(gameObjectName);
            DontDestroyOnLoad(gameObject);
            var instance = gameObject.AddComponent<PluginCoreBehaviour>();
            instance.Initialize(core);
            
            return instance;
        }
    }
}