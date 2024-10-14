using UnityEngine;

public class WebGLPluginsController : MonoBehaviour
{
    public WebGLPluginsController Instance
    {
        get; private set;
    }

    void Awake()
    {
#if !UNITY_WEBGL
        Destroy(gameObject);
        return;
#else
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
#endif
    }
}
