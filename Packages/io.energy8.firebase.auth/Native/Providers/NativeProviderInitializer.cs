using Energy8.Firebase.Auth.Native.Providers;
using UnityEngine;

namespace Energy8.Firebase.Auth.Native
{
    public static class NativeProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if !UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[FirebaseAuth.Native] Initializing Native Auth Provider");
            var provider = new NativeFirebaseAuthProvider();
            FirebaseAuth.SetProvider(provider);
#endif
        }
    }
}
