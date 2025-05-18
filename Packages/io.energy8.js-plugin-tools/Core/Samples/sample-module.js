/**
 * Sample JavaScript module for Unity Web Plugin System
 * This demonstrates how to create a JavaScript module that communicates with Unity
 */
(function() {
    'use strict';
    
    // Ensure the namespace exists
    window.UnityWebPlugin = window.UnityWebPlugin || {};
    
    // Wait for the Core module to be initialized
    document.addEventListener('UnityWebPlugin.initialized', function() {
        console.log('[SampleModule.js] UnityWebPlugin Core initialized, registering module...');
        
        // Create our module
        var sampleModule = {
            /**
             * Handles a message from Unity
             * @param {string} action - The action to perform
             * @param {string} data - The data associated with the action
             * @param {string} callbackId - Optional callback ID for responding
             */
            handleUnityMessage: function(action, data, callbackId) {
                console.log('[SampleModule.js] Received message from Unity:', action);
                
                switch(action) {
                    case 'helloResponse':
                        this.handleHelloResponse(data);
                        break;
                    
                    case 'unityInfo':
                        this.handleUnityInfo(data);
                        break;
                        
                    case 'showAlert':
                        this.handleShowAlert(data);
                        break;
                        
                    default:
                        console.warn('[SampleModule.js] Unknown action:', action);
                        break;
                }
            },
            
            /**
             * Handles a hello response from Unity
             * @param {string} dataJson - The JSON data from Unity
             */
            handleHelloResponse: function(dataJson) {
                var data = JSON.parse(dataJson);
                console.log('[SampleModule.js] Hello response from Unity:', data.message);
                
                // If we have a UI element, update it
                var responseElement = document.getElementById('unity-response');
                if (responseElement) {
                    responseElement.textContent = data.message;
                }
            },
            
            /**
             * Handles Unity information
             * @param {string} dataJson - The JSON data from Unity
             */
            handleUnityInfo: function(dataJson) {
                var data = JSON.parse(dataJson);
                console.log('[SampleModule.js] Unity Info:', data);
                
                // Display the Unity info in a formatted way
                var infoElement = document.getElementById('unity-info');
                if (infoElement) {
                    infoElement.innerHTML = 
                        '<strong>Unity Version:</strong> ' + data.unityVersion + '<br>' +
                        '<strong>Platform:</strong> ' + data.platform + '<br>' +
                        '<strong>Product:</strong> ' + data.productName + '<br>' +
                        '<strong>Company:</strong> ' + data.companyName + '<br>' +
                        '<strong>Screen:</strong> ' + data.screenWidth + 'x' + data.screenHeight;
                }
            },
            
            /**
             * Handles a show alert request from Unity
             * @param {string} dataJson - The JSON data from Unity
             */
            handleShowAlert: function(dataJson) {
                var data = JSON.parse(dataJson);
                alert(data.message);
            },
            
            /**
             * Sends a hello message to Unity
             * @param {string} name - The name to send
             */
            sayHello: function(name) {
                var data = {
                    name: name || 'JavaScript'
                };
                
                console.log('[SampleModule.js] Sending hello to Unity:', data);
                
                window.UnityWebPlugin.Core.sendMessageToUnityModule(
                    'SampleModule',
                    'sayHello',
                    data,
                    function(response) {
                        console.log('[SampleModule.js] Got response:', response);
                    }
                );
            },
            
            /**
             * Requests Unity information
             */
            getUnityInfo: function() {
                console.log('[SampleModule.js] Requesting Unity info');
                
                window.UnityWebPlugin.Core.sendMessageToUnityModule(
                    'SampleModule',
                    'getUnityInfo',
                    null
                );
            }
        };
        
        // Register our module with the Core
        window.UnityWebPlugin.Core.registerModule('SampleModule', sampleModule);
        
        // Store our module in the global namespace for easy access
        window.UnityWebPlugin.SampleModule = sampleModule;
        
        console.log('[SampleModule.js] Module registered');
        
        // Add an event handler once the Unity instance is ready
        document.addEventListener('UnityWebPlugin.moduleInitialized', function(e) {
            if (e.detail.moduleId === 'SampleModule') {
                console.log('[SampleModule.js] Unity SampleModule initialized, setting up UI');
                setupUI();
            }
        });
    });
    
    /**
     * Sets up the UI elements for interacting with the module
     */
    function setupUI() {
        // Check if we already have UI elements
        if (document.getElementById('unity-plugin-ui')) {
            return;
        }
        
        // Create UI elements
        var container = document.createElement('div');
        container.id = 'unity-plugin-ui';
        container.style.position = 'absolute';
        container.style.top = '10px';
        container.style.right = '10px';
        container.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        container.style.color = 'white';
        container.style.padding = '10px';
        container.style.borderRadius = '5px';
        container.style.fontFamily = 'Arial, sans-serif';
        container.style.zIndex = '1000';
        container.style.width = '300px';
        
        var title = document.createElement('h3');
        title.textContent = 'Unity Web Plugin - Sample Module';
        title.style.margin = '0 0 10px 0';
        container.appendChild(title);
        
        var nameInput = document.createElement('input');
        nameInput.type = 'text';
        nameInput.placeholder = 'Enter your name';
        nameInput.style.width = '100%';
        nameInput.style.padding = '5px';
        nameInput.style.marginBottom = '10px';
        nameInput.style.boxSizing = 'border-box';
        container.appendChild(nameInput);
        
        var sayHelloButton = document.createElement('button');
        sayHelloButton.textContent = 'Say Hello';
        sayHelloButton.style.padding = '5px 10px';
        sayHelloButton.style.marginRight = '10px';
        sayHelloButton.onclick = function() {
            window.UnityWebPlugin.SampleModule.sayHello(nameInput.value);
        };
        container.appendChild(sayHelloButton);
        
        var getInfoButton = document.createElement('button');
        getInfoButton.textContent = 'Get Unity Info';
        getInfoButton.style.padding = '5px 10px';
        getInfoButton.onclick = function() {
            window.UnityWebPlugin.SampleModule.getUnityInfo();
        };
        container.appendChild(getInfoButton);
        
        var responseElement = document.createElement('div');
        responseElement.id = 'unity-response';
        responseElement.style.marginTop = '10px';
        responseElement.style.padding = '5px';
        responseElement.style.backgroundColor = 'rgba(255, 255, 255, 0.1)';
        container.appendChild(responseElement);
        
        var infoElement = document.createElement('div');
        infoElement.id = 'unity-info';
        infoElement.style.marginTop = '10px';
        infoElement.style.padding = '5px';
        infoElement.style.backgroundColor = 'rgba(255, 255, 255, 0.1)';
        infoElement.style.fontSize = '12px';
        container.appendChild(infoElement);
        
        document.body.appendChild(container);
        
        console.log('[SampleModule.js] UI setup complete');
    }
})();