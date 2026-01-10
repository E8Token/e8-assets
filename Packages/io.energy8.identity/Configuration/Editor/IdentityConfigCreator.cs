#if UNITY_EDITOR
using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Editor;
using UnityEditor;

namespace Energy8.Identity.Configuration.Editor
{
    /// <summary>
    /// Context menu item for creating Identity config template
    /// Access via: Right-click in Project View → Create → E8 Config → Identity
    /// Creates a single template: IdentityConfig_Environment.asset
    /// </summary>
    public class IdentityConfigCreator : BaseModuleConfigCreator<IdentityConfig>
    {
        [MenuItem("Assets/Create/E8 Config/Identity")]
        public static void CreateConfigTemplate()
        {
            CreateTemplateConfig();
        }
    }
}
#endif
