var JSPluginDOM = {
    /**
     * Adds an event listener to an element
     * @param {string} elementId - Pointer to element ID string
     * @param {string} eventType - Pointer to event type string
     * @param {function} handler - Pointer to event handler function
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDOMAddEventListener: function(elementId, eventType, handler) {
        try {
            var id = UTF8ToString(elementId);
            var type = UTF8ToString(eventType);
            
            var element = DOMHelper.getElement(id);
            if (!element) {
                DOMHelper.logError("Element not found: " + id);
                return 0;
            }
            
            if (!DOMState.eventListeners[id]) {
                DOMState.eventListeners[id] = {};
            }
            
            DOMState.eventListeners[id][type] = handler;

            // Add the event listener
            element.addEventListener(type, handler);
            
            return 1;
        } catch (error) {
            DOMHelper.logError("Error in AddEventListener: " + error);
            return 0;
        }
    },
    
    /**
     * Removes an event listener from an element
     * @param {string} elementId - Pointer to element ID string
     * @param {string} eventType - Pointer to event type string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDOMRemoveEventListener: function(elementId, eventType) {
        try {
            var id = UTF8ToString(elementId);
            var type = UTF8ToString(eventType);
            
            var element = DOMHelper.getElement(id);
            if (!element) {
                DOMHelper.logError("Element not found: " + id);
                return 0;
            }
            
            if (DOMState.eventListeners[id] && DOMState.eventListeners[id][type]) {
                element.removeEventListener(type, DOMState.eventListeners[id][type]);
                delete DOMState.eventListeners[id][type];
                return 1;
            }
            
            return 0;
        } catch (error) {
            DOMHelper.logError("Error in RemoveEventListener: " + error);
            return 0;
        }
    },
    
    /**
     * Checks if an element is visible
     * @param {string} elementId - Pointer to element ID string
     * @return {number} 1 if visible, 0 if not visible or not found
     */
    JSPluginDOMIsElementVisible: function(elementId) {
        try {
            var id = UTF8ToString(elementId);
            var element = DOMHelper.getElement(id);
            
            if (!element) {
                DOMHelper.logError("Element not found: " + id);
                return 0;
            }
            
            // Check if element or any parent has display:none or visibility:hidden
            var current = element;
            while (current) {
                var style = window.getComputedStyle(current);
                if (style.display === 'none' || style.visibility === 'hidden') {
                    return 0;
                }
                current = current.parentElement;
            }
            
            // Check if element is in viewport
            var rect = element.getBoundingClientRect();
            if (rect.width === 0 || rect.height === 0) {
                return 0;
            }
            
            return 1;
        } catch (error) {
            DOMHelper.logError("Error in IsElementVisible: " + error);
            return 0;
        }
    },
    
    /**
     * Shows or hides an element
     * @param {string} elementId - Pointer to element ID string
     * @param {number} visible - 1 to show, 0 to hide
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDOMToggleVisibility: function(elementId, visible) {
        try {
            var id = UTF8ToString(elementId);
            var element = DOMHelper.getElement(id);
            
            if (!element) {
                DOMHelper.logError("Element not found: " + id);
                return 0;
            }
            
            element.style.display = visible ? '' : 'none';
            return 1;
        } catch (error) {
            DOMHelper.logError("Error in ToggleVisibility: " + error);
            return 0;
        }
    }
};

// Proper dependency registration
autoAddDeps(JSPluginDOM, '$DOMState');
autoAddDeps(JSPluginDOM, '$DOMHelper');
mergeInto(LibraryManager.library, JSPluginDOM);