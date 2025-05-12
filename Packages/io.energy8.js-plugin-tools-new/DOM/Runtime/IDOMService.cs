using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// Interface for manipulating browser DOM elements from Unity WebGL
    /// </summary>
    public interface IDOMService
    {
        /// <summary>
        /// Creates a new DOM element with the specified attributes
        /// </summary>
        /// <param name="tagName">HTML tag name (div, span, button, etc.)</param>
        /// <param name="id">Optional ID for the element</param>
        /// <param name="className">Optional CSS class name</param>
        /// <param name="style">Optional inline CSS styles</param>
        /// <param name="parentId">Optional parent element ID. If null, appends to body</param>
        /// <returns>The ID of the created element</returns>
        Task<string> CreateElement(string tagName, string id = null, string className = null, string style = null, string parentId = null);

        /// <summary>
        /// Sets content of an existing DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="content">HTML content to set</param>
        /// <param name="append">If true, appends content instead of replacing it</param>
        /// <returns>True if successful</returns>
        Task<bool> SetContent(string elementId, string content, bool append = false);

        /// <summary>
        /// Gets content of an existing DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <returns>The HTML content of the element</returns>
        Task<string> GetContent(string elementId);

        /// <summary>
        /// Sets attribute for a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="attributeName">Name of the attribute to set</param>
        /// <param name="attributeValue">Value of the attribute</param>
        /// <returns>True if successful</returns>
        Task<bool> SetAttribute(string elementId, string attributeName, string attributeValue);

        /// <summary>
        /// Gets attribute from a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="attributeName">Name of the attribute to get</param>
        /// <returns>Value of the attribute or null if not found</returns>
        Task<string> GetAttribute(string elementId, string attributeName);

        /// <summary>
        /// Sets CSS style for a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="propertyName">CSS property name</param>
        /// <param name="propertyValue">CSS property value</param>
        /// <returns>True if successful</returns>
        Task<bool> SetStyle(string elementId, string propertyName, string propertyValue);

        /// <summary>
        /// Gets CSS style from a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="propertyName">CSS property name</param>
        /// <returns>Value of the CSS property</returns>
        Task<string> GetStyle(string elementId, string propertyName);
        
        /// <summary>
        /// Adds an event listener to a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="eventType">Type of the event (click, mouseover, etc.)</param>
        /// <param name="eventHandler">Callback to invoke when the event is triggered</param>
        /// <returns>True if successful</returns>
        Task<bool> AddEventListener(string elementId, string eventType, Action<DOMEventData> eventHandler);
        
        /// <summary>
        /// Removes an event listener from a DOM element
        /// </summary>
        /// <param name="elementId">ID of the target element</param>
        /// <param name="eventType">Type of the event</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveEventListener(string elementId, string eventType);
        
        /// <summary>
        /// Removes a DOM element
        /// </summary>
        /// <param name="elementId">ID of the element to remove</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveElement(string elementId);
        
        /// <summary>
        /// Shows or hides a DOM element by setting its display style
        /// </summary>
        /// <param name="elementId">ID of the element</param>
        /// <param name="visible">True to show, false to hide</param>
        /// <returns>True if successful</returns>
        Task<bool> SetVisible(string elementId, bool visible);
        
        /// <summary>
        /// Finds DOM elements by CSS selector
        /// </summary>
        /// <param name="cssSelector">CSS selector to match elements</param>
        /// <returns>List of matched element IDs</returns>
        Task<List<string>> QuerySelectorAll(string cssSelector);
        
        /// <summary>
        /// Gets or sets the position of an element relative to the viewport
        /// </summary>
        /// <param name="elementId">ID of the element</param>
        /// <param name="position">Position to set (null to get only)</param>
        /// <returns>Current position of the element</returns>
        Task<DOMElementPosition> GetSetPosition(string elementId, DOMElementPosition position = null);
        
        /// <summary>
        /// Gets the size (width/height) of a DOM element
        /// </summary>
        /// <param name="elementId">ID of the element</param>
        /// <returns>Width and height of the element</returns>
        Task<Vector2> GetElementSize(string elementId);
        
        /// <summary>
        /// Creates a modal dialog
        /// </summary>
        /// <param name="options">Options for creating the modal</param>
        /// <returns>ID of the created modal element</returns>
        Task<string> CreateModal(ModalOptions options);
        
        /// <summary>
        /// Closes a modal dialog
        /// </summary>
        /// <param name="modalId">ID of the modal to close</param>
        /// <returns>True if successful</returns>
        Task<bool> CloseModal(string modalId);
    }

    /// <summary>
    /// Contains data about a DOM event
    /// </summary>
    [Serializable]
    public class DOMEventData
    {
        public string Type;
        public string ElementId;
        public float MouseX;
        public float MouseY;
        public string Value;
        public bool IsTrusted;
        public long Timestamp;
        public Dictionary<string, string> AdditionalData;
    }

    /// <summary>
    /// Position data for a DOM element
    /// </summary>
    [Serializable]
    public class DOMElementPosition
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public string Position;
    }

    /// <summary>
    /// Options for creating a modal dialog
    /// </summary>
    [Serializable]
    public class ModalOptions
    {
        public string Title;
        public string Content;
        public string Width;
        public string Height;
        public bool ShowCloseButton = true;
        public bool Draggable = false;
        public bool Backdrop = true;
        public string CustomId = null;
        public string CustomClass = null;
        public ModalButtons Buttons = null;
    }

    /// <summary>
    /// Button options for a modal dialog
    /// </summary>
    [Serializable]
    public class ModalButtons
    {
        public ModalButton[] Items;
    }

    /// <summary>
    /// Individual button for a modal dialog
    /// </summary>
    [Serializable]
    public class ModalButton
    {
        public string Text;
        public string Id;
        public string Class;
        public bool CloseOnClick = true;
    }
}