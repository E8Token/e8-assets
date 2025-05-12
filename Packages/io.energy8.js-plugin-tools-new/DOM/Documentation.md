# DOM Module Documentation

## Overview

The DOM Module provides a bridge between Unity and the browser's Document Object Model (DOM), allowing Unity applications to manipulate web page elements directly. This module enables Unity WebGL applications to create, modify, and interact with HTML elements seamlessly.

## Features

- **DOM Element Creation**: Create HTML elements from Unity code
- **Element Manipulation**: Change content, attributes, and styles of DOM elements
- **Event Handling**: Register and respond to DOM events (clicks, input, etc.)
- **Element Queries**: Find elements using CSS selectors
- **Position and Size**: Get or set element positions and dimensions
- **Modal Dialogs**: Create and manage custom modal dialogs
- **Visibility Control**: Show or hide elements easily

## Classes and Components

### IDOMService

The main interface for manipulating DOM elements:

```csharp
public interface IDOMService
{
    Task<string> CreateElement(string tagName, string id = null, string className = null, string style = null, string parentId = null);
    Task<List<string>> GetCreatedElements();
    Task SetContent(string elementId, string content, bool append = false);
    Task<string> GetContent(string elementId);
    Task SetAttribute(string elementId, string attributeName, string attributeValue);
    Task<string> GetAttribute(string elementId, string attributeName);
    Task SetStyle(string elementId, string propertyName, string propertyValue);
    Task<string> GetStyle(string elementId, string propertyName);
    Task AddEventListener(string elementId, string eventType, Action<DOMEventData> callback);
    Task RemoveEventListener(string elementId, string eventType);
    Task RemoveElement(string elementId);
    Task SetVisible(string elementId, bool visible);
    Task<List<string>> QuerySelectorAll(string cssSelector);
    Task<DOMRect> GetElementPosition(string elementId);
    Task SetElementPosition(string elementId, DOMRect position);
    Task<DOMRect> GetElementSize(string elementId);
    Task<string> CreateModal(ModalOptions options);
    Task CloseModal(string modalId);
}
```

### DOMService

The implementation of `IDOMService` that provides DOM manipulation capabilities.

### IDOMManager

Higher-level interface for DOM operations:

```csharp
public interface IDOMManager
{
    void Initialize(IPluginCore core);
    bool IsInitialized { get; }
    event Action OnInitialized;
    Task<string> CreateElement(string tagName, string id = null, string className = null, string style = null, string parentId = null);
    Task<List<string>> GetAllElements();
    // Similar methods to IDOMService with additional functionality
}
```

### DOMManager

The implementation of `IDOMManager` that coordinates DOM operations through the plugin core.

### DOMServiceBehaviour

A MonoBehaviour wrapper for DOMService that allows finding it with `FindObjectOfType`.

### DOMEventHandler

MonoBehaviour component that handles DOM events and forwards them to appropriate callbacks.

### DOMRect

Class representing the position and dimensions of DOM elements:

```csharp
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
```

### ModalOptions

Class for configuring modal dialogs:

```csharp
public class ModalOptions
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Width { get; set; } = "400px";
    public string Height { get; set; } = "auto";
    public bool CloseOnClickOutside { get; set; } = true;
    public bool Draggable { get; set; } = true;
    public string Position { get; set; } = "center";
    public List<ModalButton> Buttons { get; set; }
}
```

## Usage

### Basic DOM Manipulation

```csharp
// Get or create the DOM manager
var domManager = new DOMManager();
domManager.Initialize(pluginCore);

// Create an HTML button element
string buttonId = await domManager.CreateElement(
    "button", 
    "myButton", 
    "btn btn-primary", 
    "margin: 10px; padding: 5px 10px;"
);

// Set button text
await domManager.SetContent(buttonId, "Click Me!");

// Add a click event listener
await domManager.AddEventListener(buttonId, "click", (eventData) => {
    Debug.Log("Button clicked!");
    // You can access event data properties:
    // eventData.ClientX, eventData.ClientY, etc.
});
```

### Creating and Using a Form

```csharp
// Create a form element
string formId = await domManager.CreateElement("form", "myForm");

// Create an input field
string inputId = await domManager.CreateElement(
    "input", 
    "nameInput", 
    "form-control", 
    null, 
    formId
);

// Set input attributes
await domManager.SetAttribute(inputId, "type", "text");
await domManager.SetAttribute(inputId, "placeholder", "Enter your name");

// Create a submit button inside the form
string submitId = await domManager.CreateElement(
    "button", 
    "submitBtn", 
    "btn btn-success", 
    null, 
    formId
);
await domManager.SetContent(submitId, "Submit");
await domManager.SetAttribute(submitId, "type", "button");

// Add event listener for the submit button
await domManager.AddEventListener(submitId, "click", async (e) => {
    // Get the input value
    string name = await domManager.GetValue(inputId);
    Debug.Log($"Submitted name: {name}");
});
```

### Creating a Modal Dialog

```csharp
// Create modal options
var options = new ModalOptions {
    Title = "Game Over",
    Content = "<p>Your score: 1250</p><p>Would you like to play again?</p>",
    Width = "300px",
    Draggable = true,
    Buttons = new List<ModalButton> {
        new ModalButton { Text = "Play Again", Id = "play-again", Style = "primary" },
        new ModalButton { Text = "Quit", Id = "quit", Style = "danger" }
    }
};

// Show the modal
string modalId = await domManager.CreateModal(options);

// Add event listeners for the buttons
await domManager.AddEventListener("play-again", "click", (e) => {
    // Handle play again
    RestartGame();
    domManager.CloseModal(modalId);
});

await domManager.AddEventListener("quit", "click", (e) => {
    // Handle quit
    ReturnToMainMenu();
    domManager.CloseModal(modalId);
});
```

### Finding Elements in the DOM

```csharp
// Find all elements with a specific class
List<string> elements = await domManager.QuerySelectorAll(".game-ui-element");

// Modify these elements
foreach (string id in elements) {
    await domManager.SetStyle(id, "color", "red");
}
```

## JavaScript API

In JavaScript, the DOM module provides these methods:

```javascript
// Create a DOM element
const elementId = Energy8JSPluginTools.DOM.createElement("div", "myDiv", "custom-class", "color: blue;");

// Set content of an element
Energy8JSPluginTools.DOM.setContent("myDiv", "<strong>Hello World!</strong>");

// Add an event listener
Energy8JSPluginTools.DOM.addEventListener("myDiv", "click");

// Get all elements created by the DOM module
const elements = Energy8JSPluginTools.DOM.getCreatedElements();

// Create a modal dialog
const modalId = Energy8JSPluginTools.DOM.createModal({
    title: "JavaScript Modal",
    content: "Created from JavaScript",
    width: "400px",
    buttons: [
        { text: "OK", id: "ok-btn", style: "primary" }
    ]
});
```

## Best Practices

1. Use meaningful IDs for DOM elements to make them easier to manage.
2. Always remove event listeners and elements when they are no longer needed.
3. Consider using existing CSS frameworks (like Bootstrap) by including them in your HTML page.
4. Set the `position` CSS property appropriately for elements you want to position precisely.
5. Use the async/await pattern for DOM operations to ensure proper sequence execution.
6. Be careful with modal dialogs in WebGL - they should be used sparingly and designed to fit well with your game's UI.
7. Remember that DOM manipulation affects the browser's layout calculations, so avoid excessive DOM changes in performance-critical code.
8. When handling events, use debounce techniques for events that fire frequently (like resize or mousemove).
9. Consider mobile devices when designing your DOM elements (touch-friendly sizes, responsive layouts).