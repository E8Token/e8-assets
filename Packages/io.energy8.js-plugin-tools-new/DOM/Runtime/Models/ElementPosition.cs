using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// Представляет информацию о позиции DOM-элемента
    /// </summary>
    [Serializable]
    public class ElementPosition
    {
        /// <summary>
        /// Позиция элемента слева
        /// </summary>
        [JsonProperty("Left")]
        public int Left { get; set; }
        
        /// <summary>
        /// Позиция элемента сверху
        /// </summary>
        [JsonProperty("Top")]
        public int Top { get; set; }
        
        /// <summary>
        /// Позиция элемента справа
        /// </summary>
        [JsonProperty("Right")]
        public int Right { get; set; }
        
        /// <summary>
        /// Позиция элемента снизу
        /// </summary>
        [JsonProperty("Bottom")]
        public int Bottom { get; set; }
        
        /// <summary>
        /// Тип позиционирования (absolute, relative, fixed, static)
        /// </summary>
        [JsonProperty("Position")]
        public string Position { get; set; }
        
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ElementPosition()
        {
        }
        
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="left">Позиция слева</param>
        /// <param name="top">Позиция сверху</param>
        public ElementPosition(int left, int top)
        {
            Left = left;
            Top = top;
        }
        
        /// <summary>
        /// Конструктор с полным набором параметров
        /// </summary>
        /// <param name="left">Позиция слева</param>
        /// <param name="top">Позиция сверху</param>
        /// <param name="right">Позиция справа</param>
        /// <param name="bottom">Позиция снизу</param>
        /// <param name="position">Тип позиционирования</param>
        public ElementPosition(int left, int top, int right, int bottom, string position)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Position = position;
        }
        
        /// <summary>
        /// Ширина элемента
        /// </summary>
        public int Width => Right - Left;
        
        /// <summary>
        /// Высота элемента
        /// </summary>
        public int Height => Bottom - Top;
        
        /// <summary>
        /// Получает размер элемента как Vector2
        /// </summary>
        public Vector2 Size => new Vector2(Width, Height);
        
        /// <summary>
        /// Получает позицию элемента как Vector2 (левый верхний угол)
        /// </summary>
        public Vector2 PositionVector => new Vector2(Left, Top);
    }
}