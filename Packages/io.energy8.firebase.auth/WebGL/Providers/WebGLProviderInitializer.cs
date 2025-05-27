using Energy8.Firebase.Auth.WebGL.Providers;
using UnityEngine;

namespace Energy8.Firebase.Auth.WebGL
{
    public static class WebGLProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[FirebaseAuth.WebGL] Initializing WebGL Auth Provider");
            var provider = new WebFirebaseAuthProvider();
            FirebaseAuth.SetProvider(provider);
#endif
        }
    }
}
