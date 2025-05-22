using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Base class for all JavaScript plugins.
    /// Plugins inherit from this class to integrate with the JavaScript interop system.
    /// </summary>
    public abstract class JSPluginBase : MonoBehaviour
    {
        /// <summary>
        /// Gets the unique plugin identifier.
        /// </summary>
        public abstract string PluginName { get; }

        /// <summary>
        /// Initializes the plugin instance.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Handles a message from the JavaScript side.
        /// </summary>
        /// <param name="method">The method name being called</param>
        /// <param name="payload">The JSON payload containing the method parameters</param>
        public virtual void OnMessage(string method, string payload) { }
    }
}
