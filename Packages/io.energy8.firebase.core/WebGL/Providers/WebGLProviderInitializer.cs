#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Firebase.Core;
using UnityEngine;

namespace Energy8.Firebase.Core.WebGL
{
    /// <summary>
    /// Auto-initializer for WebGL Firebase Core Provider
    /// </summary>
    internal static class WebGLProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("[FirebaseCore] Initializing WebGL provider");
            FirebaseCore.RegisterProvider(new WebFirebaseCoreProvider());
        }
    }
}
#endif
