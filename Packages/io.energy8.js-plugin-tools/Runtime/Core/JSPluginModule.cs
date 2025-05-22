namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Base class for all JavaScript plugin modules.
    /// Modules provide additional functionality to the core plugin system.
    /// </summary>
    public abstract class JSPluginModule
    {
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        /// Called once during module registration.
        /// </summary>
        public virtual void InitializeModule() { }
    }
}
