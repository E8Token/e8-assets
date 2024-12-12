using UnityEngine;
using System.Runtime.InteropServices;

namespace Energy8.Plugins.WebGL.HSL
{
    public class HLSPlayerWebGL : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void InitializePlayer();

        [DllImport("__Internal")]
        public static extern void SetUrl(string url);

        [DllImport("__Internal")]
        public static extern void Play();

        [DllImport("__Internal")]
        public static extern void Pause();

        [DllImport("__Internal")]
        public static extern void SetHLSVolume(float volume);
    }
}
