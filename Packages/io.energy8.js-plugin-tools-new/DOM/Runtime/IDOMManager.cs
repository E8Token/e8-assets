using System;
using System.Threading.Tasks;
using UnityEngine;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// Интерфейс для взаимодействия с DOM-моделью веб-страницы
    /// </summary>
    public interface IDOMManager
    {
        /// <summary>
        /// Инициализирует модуль DOM с указанным ядром плагина
        /// </summary>
        /// <param name="core">Экземпляр ядра плагина</param>
        void Initialize(IPluginCore core);
        
        /// <summary>
        /// Проверяет, инициализирован ли модуль
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Событие, возникающее при инициализации модуля
        /// </summary>
        event Action OnInitialized;
        
        /// <summary>
        /// Создает HTML-элемент и добавляет его на страницу
        /// </summary>
        /// <param name="elementType">Тип элемента (div, button, span и т.д.)</param>
        /// <param name="id">ID элемента</param>
        /// <param name="parentSelector">CSS-селектор родительского элемента (если null, добавляется в body)</param>
        /// <param name="attributes">Атрибуты элемента в формате JSON</param>
        /// <param name="styles">Стили элемента в формате JSON</param>
        /// <param name="content">Содержимое элемента (innerHTML)</param>
        /// <returns>True, если элемент успешно создан</returns>
        Task<bool> CreateElementAsync(string elementType, string id, string parentSelector = null, string attributes = null, string styles = null, string content = null);
        
        /// <summary>
        /// Удаляет элемент со страницы
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <returns>True, если элемент успешно удален</returns>
        Task<bool> RemoveElementAsync(string selector);
        
        /// <summary>
        /// Обновляет содержимое элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="content">Новое содержимое (innerHTML)</param>
        /// <returns>True, если содержимое успешно обновлено</returns>
        Task<bool> UpdateContentAsync(string selector, string content);
        
        /// <summary>
        /// Обновляет атрибуты элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="attributes">Атрибуты в формате JSON</param>
        /// <returns>True, если атрибуты успешно обновлены</returns>
        Task<bool> UpdateAttributesAsync(string selector, string attributes);
        
        /// <summary>
        /// Обновляет стили элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="styles">Стили в формате JSON</param>
        /// <returns>True, если стили успешно обновлены</returns>
        Task<bool> UpdateStylesAsync(string selector, string styles);
        
        /// <summary>
        /// Добавляет CSS-класс к элементу
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="className">Имя класса</param>
        /// <returns>True, если класс успешно добавлен</returns>
        Task<bool> AddClassAsync(string selector, string className);
        
        /// <summary>
        /// Удаляет CSS-класс у элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="className">Имя класса</param>
        /// <returns>True, если класс успешно удален</returns>
        Task<bool> RemoveClassAsync(string selector, string className);
        
        /// <summary>
        /// Переключает CSS-класс у элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="className">Имя класса</param>
        /// <returns>True, если класс успешно переключен</returns>
        Task<bool> ToggleClassAsync(string selector, string className);
        
        /// <summary>
        /// Получает значение атрибута элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="attributeName">Имя атрибута</param>
        /// <returns>Значение атрибута</returns>
        Task<string> GetAttributeAsync(string selector, string attributeName);
        
        /// <summary>
        /// Получает содержимое элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="contentType">Тип содержимого (innerHTML, innerText, value, textContent)</param>
        /// <returns>Содержимое элемента</returns>
        Task<string> GetContentAsync(string selector, string contentType = "innerHTML");
        
        /// <summary>
        /// Проверяет существование элемента на странице
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <returns>True, если элемент существует</returns>
        Task<bool> ElementExistsAsync(string selector);
        
        /// <summary>
        /// Добавляет обработчик события для элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <param name="eventName">Название события (click, change и т.д.)</param>
        /// <param name="callbackName">Имя метода обратного вызова в Unity</param>
        /// <returns>ID обработчика события</returns>
        Task<string> AddEventListenerAsync(string selector, string eventName, string callbackName);
        
        /// <summary>
        /// Удаляет обработчик события для элемента
        /// </summary>
        /// <param name="handlerId">ID обработчика события</param>
        /// <returns>True, если обработчик успешно удален</returns>
        Task<bool> RemoveEventListenerAsync(string handlerId);
        
        /// <summary>
        /// Добавляет CSS на страницу
        /// </summary>
        /// <param name="cssText">CSS-код</param>
        /// <param name="id">ID стилевого элемента</param>
        /// <returns>True, если стили успешно добавлены</returns>
        Task<bool> AddCSSAsync(string cssText, string id = null);
        
        /// <summary>
        /// Получает информацию о размерах и положении элемента
        /// </summary>
        /// <param name="selector">CSS-селектор элемента</param>
        /// <returns>Информация о размерах и положении</returns>
        Task<DOMRect> GetBoundingClientRectAsync(string selector);
    }
    
    /// <summary>
    /// Представляет размеры и положение элемента
    /// </summary>
    [Serializable]
    public class DOMRect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
    }
}