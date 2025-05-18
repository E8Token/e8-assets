using System;
using UnityEngine;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Defines the contract for any plugin module in the JS Plugin Tools system.
    /// Modules must implement this interface to be registered and managed by the plugin system.
    /// </summary>
    public interface IPluginModule
    {
        /// <summary>
        /// Gets the unique identifier for this plugin module.
        /// </summary>
        string ModuleId { get; }
        
        /// <summary>
        /// Gets a value indicating whether this module is initialized.
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initializes this module.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        bool Initialize();
        
        /// <summary>
        /// Shuts down this module, releasing any resources it holds.
        /// </summary>
        void Shutdown();
    }
}
