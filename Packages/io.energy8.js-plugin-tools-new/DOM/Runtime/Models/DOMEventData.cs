using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.DOM.NewModels
{
    /// <summary>
    /// Представляет данные о событии DOM, полученные от JavaScript
    /// </summary>
    [Serializable]
    public class DOMEventData
    {
        /// <summary>
        /// Тип события (click, keydown, input и т.д.)
        /// </summary>
        [JsonProperty("Type")]
        public string Type { get; set; }
        
        /// <summary>
        /// ID элемента, на котором произошло событие
        /// </summary>
        [JsonProperty("ElementId")]
        public string ElementId { get; set; }
        
        /// <summary>
        /// X координата мыши (для событий мыши)
        /// </summary>
        [JsonProperty("MouseX")]
        public float MouseX { get; set; }
        
        /// <summary>
        /// Y координата мыши (для событий мыши)
        /// </summary>
        [JsonProperty("MouseY")]
        public float MouseY { get; set; }
        
        /// <summary>
        /// Значение элемента (для input, textarea, select и т.д.)
        /// </summary>
        [JsonProperty("Value")]
        public string Value { get; set; }
        
        /// <summary>
        /// Флаг, указывающий, было ли событие инициировано пользователем
        /// </summary>
        [JsonProperty("IsTrusted")]
        public bool IsTrusted { get; set; }
        
        /// <summary>
        /// Временная метка события
        /// </summary>
        [JsonProperty("Timestamp")]
        public double Timestamp { get; set; }
        
        /// <summary>
        /// Дополнительные данные события (зависят от типа события)
        /// </summary>
        [JsonProperty("AdditionalData")]
        public Dictionary<string, object> AdditionalData { get; set; }
        
        /// <summary>
        /// Преобразует координаты мыши в Vector2
        /// </summary>
        public Vector2 MousePosition => new Vector2(MouseX, MouseY);
        
        /// <summary>
        /// Пытается получить значение из AdditionalData как bool
        /// </summary>
        /// <param name="key">Ключ для поиска значения</param>
        /// <param name="defaultValue">Значение по умолчанию, если ключ не найден или значение не может быть преобразовано</param>
        /// <returns>Значение как bool</returns>
        public bool GetBoolValue(string key, bool defaultValue = false)
        {
            if (AdditionalData == null || !AdditionalData.ContainsKey(key))
            {
                return defaultValue;
            }
            
            try
            {
                return Convert.ToBoolean(AdditionalData[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Пытается получить значение из AdditionalData как int
        /// </summary>
        /// <param name="key">Ключ для поиска значения</param>
        /// <param name="defaultValue">Значение по умолчанию, если ключ не найден или значение не может быть преобразовано</param>
        /// <returns>Значение как int</returns>
        public int GetIntValue(string key, int defaultValue = 0)
        {
            if (AdditionalData == null || !AdditionalData.ContainsKey(key))
            {
                return defaultValue;
            }
            
            try
            {
                return Convert.ToInt32(AdditionalData[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Пытается получить значение из AdditionalData как float
        /// </summary>
        /// <param name="key">Ключ для поиска значения</param>
        /// <param name="defaultValue">Значение по умолчанию, если ключ не найден или значение не может быть преобразовано</param>
        /// <returns>Значение как float</returns>
        public float GetFloatValue(string key, float defaultValue = 0f)
        {
            if (AdditionalData == null || !AdditionalData.ContainsKey(key))
            {
                return defaultValue;
            }
            
            try
            {
                return Convert.ToSingle(AdditionalData[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Пытается получить значение из AdditionalData как string
        /// </summary>
        /// <param name="key">Ключ для поиска значения</param>
        /// <param name="defaultValue">Значение по умолчанию, если ключ не найден или значение не может быть преобразовано</param>
        /// <returns>Значение как string</returns>
        public string GetStringValue(string key, string defaultValue = null)
        {
            if (AdditionalData == null || !AdditionalData.ContainsKey(key))
            {
                return defaultValue;
            }
            
            return AdditionalData[key]?.ToString() ?? defaultValue;
        }
        
        /// <summary>
        /// Проверяет, был ли нажат указанный ключ в событии клавиатуры
        /// </summary>
        /// <param name="key">Код или название клавиши для проверки</param>
        /// <returns>true если ключ совпадает с событием, иначе false</returns>
        public bool IsKey(string key)
        {
            if (Type != "keydown" && Type != "keyup" && Type != "keypress")
            {
                return false;
            }
            
            string eventKey = GetStringValue("key");
            int eventKeyCode = GetIntValue("keyCode");
            
            if (!string.IsNullOrEmpty(eventKey) && eventKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // Проверка по коду клавиши
            if (int.TryParse(key, out int keyCode) && eventKeyCode == keyCode)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Проверяет, была ли нажата модификаторная клавиша (Alt, Ctrl, Shift)
        /// </summary>
        /// <param name="modifier">Название модификатора: "alt", "ctrl", "shift"</param>
        /// <returns>true если модификатор активен, иначе false</returns>
        public bool HasModifier(string modifier)
        {
            if (Type != "keydown" && Type != "keyup" && Type != "keypress" && 
                Type != "click" && Type != "mousedown" && Type != "mouseup")
            {
                return false;
            }
            
            switch (modifier.ToLowerInvariant())
            {
                case "alt":
                    return GetBoolValue("altKey");
                case "ctrl":
                case "control":
                    return GetBoolValue("ctrlKey");
                case "shift":
                    return GetBoolValue("shiftKey");
                default:
                    return false;
            }
        }
    }
}