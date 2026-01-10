using UnityEngine;

namespace Energy8.EnvironmentConfig.Base
{
    /// <summary>
    /// Base class for all module configuration assets
    /// Each module should create its own config class inheriting from this
    /// </summary>
    public abstract class BaseModuleConfig : ScriptableObject
    {
        /// <summary>
        /// Log configuration details at startup
        /// Override in derived classes to add module-specific logging
        /// </summary>
        public virtual void LogConfigInfo()
        {
            string className = GetType().Name;
            Debug.Log($"[{className}] Configuration loaded");
        }
    }
}
