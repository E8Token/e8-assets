using System.Runtime.InteropServices;

namespace Energy8.Plugins.WebGL.LocalStorage
{
    public class LocalStorageController
    {
        [DllImport("__Internal")]
        public static extern bool HasKey(string key);
        [DllImport("__Internal")]
        public static extern string Get(string key);
        [DllImport("__Internal")]
        public static extern void Set(string key, string value);
        [DllImport("__Internal")]
        public static extern void Remove(string key);
    }
}