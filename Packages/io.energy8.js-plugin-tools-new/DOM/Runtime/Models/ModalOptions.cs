using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Energy8.JSPluginTools.DOM.NewModels
{
    /// <summary>
    /// Опции для создания модального окна
    /// </summary>
    [Serializable]
    public class ModalOptions
    {
        /// <summary>
        /// Пользовательский идентификатор для модального окна
        /// </summary>
        [JsonProperty("CustomId")]
        public string CustomId { get; set; }
        
        /// <summary>
        /// Заголовок модального окна
        /// </summary>
        [JsonProperty("Title")]
        public string Title { get; set; }
        
        /// <summary>
        /// HTML-содержимое модального окна
        /// </summary>
        [JsonProperty("Content")]
        public string Content { get; set; }
        
        /// <summary>
        /// Пользовательский CSS-класс для модального окна
        /// </summary>
        [JsonProperty("CustomClass")]
        public string CustomClass { get; set; }
        
        /// <summary>
        /// Ширина модального окна (в пикселях или CSS-формате)
        /// </summary>
        [JsonProperty("Width")]
        public string Width { get; set; }
        
        /// <summary>
        /// Высота модального окна (в пикселях или CSS-формате)
        /// </summary>
        [JsonProperty("Height")]
        public string Height { get; set; }
        
        /// <summary>
        /// Показывать ли затемнение фона
        /// </summary>
        [JsonProperty("Backdrop")]
        public bool? Backdrop { get; set; } = true;
        
        /// <summary>
        /// Показывать ли кнопку закрытия
        /// </summary>
        [JsonProperty("ShowCloseButton")]
        public bool? ShowCloseButton { get; set; } = true;
        
        /// <summary>
        /// Можно ли перетаскивать модальное окно
        /// </summary>
        [JsonProperty("Draggable")]
        public bool Draggable { get; set; }
        
        /// <summary>
        /// Кнопки модального окна
        /// </summary>
        [JsonProperty("Buttons")]
        public ModalButtons Buttons { get; set; }
        
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ModalOptions()
        {
        }
        
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="title">Заголовок модального окна</param>
        /// <param name="content">HTML-содержимое модального окна</param>
        public ModalOptions(string title, string content)
        {
            Title = title;
            Content = content;
        }
        
        /// <summary>
        /// Добавляет кнопку в модальное окно
        /// </summary>
        /// <param name="text">Текст кнопки</param>
        /// <param name="id">Идентификатор кнопки</param>
        /// <param name="cssClass">CSS-класс для кнопки</param>
        /// <param name="closeOnClick">Закрывать ли модальное окно при нажатии на кнопку</param>
        /// <returns>Текущий экземпляр ModalOptions для fluent API</returns>
        public ModalOptions AddButton(string text, string id = null, string cssClass = null, bool closeOnClick = true)
        {
            if (Buttons == null)
            {
                Buttons = new ModalButtons();
            }
            
            if (Buttons.Items == null)
            {
                Buttons.Items = new List<ModalButton>();
            }
            
            Buttons.Items.Add(new ModalButton
            {
                Text = text,
                Id = id,
                Class = cssClass,
                CloseOnClick = closeOnClick
            });
            
            return this;
        }
        
        /// <summary>
        /// Создает простое информационное модальное окно с кнопкой "ОК"
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="message">Сообщение</param>
        /// <param name="buttonText">Текст кнопки (по умолчанию "ОК")</param>
        /// <returns>Настроенный объект ModalOptions</returns>
        public static ModalOptions CreateInfo(string title, string message, string buttonText = "OK")
        {
            var options = new ModalOptions
            {
                Title = title,
                Content = message,
                ShowCloseButton = false
            };
            
            return options.AddButton(buttonText);
        }
        
        /// <summary>
        /// Создает модальное окно подтверждения с кнопками "Да" и "Нет"
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="message">Сообщение</param>
        /// <param name="yesButtonText">Текст кнопки "Да" (по умолчанию "Да")</param>
        /// <param name="noButtonText">Текст кнопки "Нет" (по умолчанию "Нет")</param>
        /// <returns>Настроенный объект ModalOptions</returns>
        public static ModalOptions CreateConfirm(string title, string message, 
            string yesButtonText = "Да", string noButtonText = "Нет")
        {
            var options = new ModalOptions
            {
                Title = title,
                Content = message,
                ShowCloseButton = false
            };
            
            return options
                .AddButton(yesButtonText, "confirm-yes", "confirm-yes")
                .AddButton(noButtonText, "confirm-no", "confirm-no");
        }
    }
    
    /// <summary>
    /// Контейнер для кнопок модального окна
    /// </summary>
    [Serializable]
    public class ModalButtons
    {
        /// <summary>
        /// Список кнопок модального окна
        /// </summary>
        [JsonProperty("Items")]
        public List<ModalButton> Items { get; set; } = new List<ModalButton>();
    }
    
    /// <summary>
    /// Представляет кнопку модального окна
    /// </summary>
    [Serializable]
    public class ModalButton
    {
        /// <summary>
        /// Текст кнопки
        /// </summary>
        [JsonProperty("Text")]
        public string Text { get; set; }
        
        /// <summary>
        /// Идентификатор кнопки
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }
        
        /// <summary>
        /// CSS-класс для кнопки
        /// </summary>
        [JsonProperty("Class")]
        public string Class { get; set; }
        
        /// <summary>
        /// Закрывать ли модальное окно при нажатии на кнопку
        /// </summary>
        [JsonProperty("CloseOnClick")]
        public bool CloseOnClick { get; set; } = true;
    }
}