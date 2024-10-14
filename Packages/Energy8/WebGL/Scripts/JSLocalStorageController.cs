using System.Runtime.InteropServices;

public class JSLocalStorageController
{
    [DllImport("__Internal")]
    public static extern string Get(string key);
    [DllImport("__Internal")]
    public static extern void Set(string key, string value);
    [DllImport("__Internal")]
    public static extern void Remove(string key);
}
