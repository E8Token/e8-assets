using UnityEngine;

namespace Energy8.WebGL.PluginPlatform
{
    /// <summary>
    /// Базовый абстрактный класс для всех WebGL плагинов
    /// </summary>
    public abstract class BasePlugin : MonoBehaviour
    {
        [SerializeField] private int priority = 50;
        [SerializeField] private string pluginName;
        [SerializeField] private string version = "1.0.0";
        [SerializeField] private bool isEnabled = true;
        
        /// <summary>
        /// Приоритет загрузки плагина (0-100, где 0 - наивысший приоритет)
        /// </summary>
        public int Priority 
        { 
            get => priority; 
            set => priority = Mathf.Clamp(value, 0, 100); 
        }
        
        /// <summary>
        /// Название плагина
        /// </summary>
        public string PluginName 
        { 
            get => string.IsNullOrEmpty(pluginName) ? GetType().Name : pluginName; 
            set => pluginName = value; 
        }
        
        /// <summary>
        /// Версия плагина
        /// </summary>
        public string Version 
        { 
            get => version; 
            set => version = value; 
        }
        
        /// <summary>
        /// Включен ли плагин
        /// </summary>
        public bool IsEnabled 
        { 
            get => isEnabled; 
            set => isEnabled = value; 
        }
        
        /// <summary>
        /// Настройки плагина
        /// </summary>
        public abstract IPluginSettings Settings { get; }
        
        /// <summary>
        /// Инициализация плагина
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// Включение плагина
        /// </summary>
        public abstract void Enable();
        
        /// <summary>
        /// Выключение плагина
        /// </summary>
        public abstract void Disable();
        
        /// <summary>
        /// Уничтожение плагина
        /// </summary>
        public abstract void Destroy();
        
        protected virtual void Awake()
        {
            if (PluginManager.Instance != null)
            {
                PluginManager.Instance.RegisterPlugin(this);
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (PluginManager.Instance != null)
            {
                PluginManager.Instance.UnregisterPlugin(this);
            }
        }
    }
}
