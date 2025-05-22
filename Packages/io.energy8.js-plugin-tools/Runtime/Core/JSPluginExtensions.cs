using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Provides extension methods for JavaScript plugin communication.
    /// </summary>
    public static class JSPluginExtensions
    {
        /// <summary>
        /// Sends a message to the JavaScript side.
        /// </summary>
        /// <param name="plugin">The plugin sending the message.</param>
        /// <param name="method">The method to call.</param>
        /// <param name="payload">The JSON payload.</param>
        public static void SendMessageToJS(this JSPluginBase plugin, string method, string payload)
        {
            if (plugin == null)
            {
                Debug.LogError("Cannot send message from null plugin");
                return;
            }
            
            JSPluginManager.Instance.SendMessage(plugin.PluginName, method, payload);
        }
    }
}
