#if !UNITY_WEBGL || UNITY_EDITOR
using Energy8.Firebase.Core;
using UnityEngine;

namespace Energy8.Firebase.Core.Providers
{
    /// <summary>
    /// Auto-initializer for Native Firebase Core Provider
    /// </summary>
    internal static class NativeProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("[FirebaseCore] Initializing Native provider");
            FirebaseCore.RegisterProvider(new NativeFirebaseCoreProvider());
        }
    }
}
#endif
