using System;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Core interface for managing the lifecycle of the JS plugin system
    /// </summary>
    public interface IPluginCore
    {
        /// <summary>
        /// Initializes the plugin system
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shuts down the plugin system and releases resources
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Checks if the plugin system is running in a WebGL context
        /// </summary>
        bool IsWebGLContext { get; }

        /// <summary>
        /// Checks if the plugin system has been initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Event triggered when the plugin is fully initialized
        /// </summary>
        event Action OnInitialized;

        /// <summary>
        /// Event triggered when the plugin is being shut down
        /// </summary>
        event Action OnShutdown;
        
        /// <summary>
        /// Registers a game object for communication with JavaScript
        /// </summary>
        /// <param name="gameObjectName">Name of the GameObject to register</param>
        /// <returns>Object ID that can be used for JS communication</returns>
        string RegisterGameObject(string gameObjectName);
        
        /// <summary>
        /// Unregisters a previously registered game object
        /// </summary>
        /// <param name="objectId">Object ID returned from RegisterGameObject</param>
        void UnregisterGameObject(string objectId);
    }
}