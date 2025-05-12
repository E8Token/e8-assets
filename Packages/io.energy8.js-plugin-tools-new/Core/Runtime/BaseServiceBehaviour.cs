using UnityEngine;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Base class for all JSPluginTools service behaviour components
    /// </summary>
    public abstract class BaseServiceBehaviour : MonoBehaviour
    {
        protected IPluginCore _pluginCore;
        protected bool _isInitialized = false;

        /// <summary>
        /// Indicates whether the service is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Initialize the service with core dependency
        /// </summary>
        /// <param name="pluginCore">Plugin core instance</param>
        public virtual void Initialize(IPluginCore pluginCore)
        {
            _pluginCore = pluginCore ?? throw new System.ArgumentNullException(nameof(pluginCore));
            _isInitialized = true;
            
            Debug.Log($"JSPluginTools [{GetType().Name}]: Initialized");
        }

        /// <summary>
        /// Checks if the service is initialized and throws an exception if not
        /// </summary>
        protected void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new System.InvalidOperationException($"JSPluginTools [{GetType().Name}]: Not initialized. Call Initialize() first.");
            }
        }
        
        /// <summary>
        /// Cleanup resources when the component is destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            _isInitialized = false;
            Debug.Log($"JSPluginTools [{GetType().Name}]: Destroyed");
        }
    }
}