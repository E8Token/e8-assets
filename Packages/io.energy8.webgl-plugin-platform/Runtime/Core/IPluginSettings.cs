using UnityEngine;

namespace Energy8.WebGL.PluginPlatform
{
    /// <summary>
    /// Базовый интерфейс для настроек плагинов
    /// </summary>
    public interface IPluginSettings
    {
        /// <summary>
        /// Сериализация настроек в JSON
        /// </summary>
        string ToJson();
        
        /// <summary>
        /// Десериализация настроек из JSON
        /// </summary>
        void FromJson(string json);
        
        /// <summary>
        /// Сброс настроек к значениям по умолчанию
        /// </summary>
        void ResetToDefaults();
    }
}
