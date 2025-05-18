using UnityEngine;

namespace Energy8.JSPluginTools.Core.Samples
{
    /// <summary>
    /// Sample module demonstrating how to use the JS Plugin Tools system.
    /// </summary>
    public class SampleModule : JSMessageHandlerModuleBase
    {
        /// <summary>
        /// Gets the unique identifier for this module.
        /// </summary>
        public override string ModuleId => "SampleModule";
        
        /// <summary>
        /// Called when the module is being initialized.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        protected override bool OnInitialize()
        {
            Debug.Log("[SampleModule] Initializing...");
            
            // Register action handlers
            RegisterActionHandler("sayHello", HandleSayHello);
            RegisterActionHandler("getUnityInfo", HandleGetUnityInfo);
            
            Debug.Log("[SampleModule] Initialized successfully");
            return true;
        }
        
        /// <summary>
        /// Called when the module is being shut down.
        /// </summary>
        protected override void OnShutdown()
        {
            Debug.Log("[SampleModule] Shutting down...");
        }
        
        /// <summary>
        /// Handles the "sayHello" action from JavaScript.
        /// </summary>
        /// <param name="json">The JSON data from JavaScript.</param>
        private void HandleSayHello(string json)
        {
            Debug.Log($"[SampleModule] Received sayHello message: {json}");
            
            // Parse the JSON data
            var data = JsonUtility.FromJson<HelloMessage>(json);
            
            // Send a response back to JavaScript
            var response = new HelloResponse
            {
                message = $"Hello from Unity, {data.name}!"
            };
            
            SendMessageToJS("helloResponse", JsonUtility.ToJson(response));
        }
        
        /// <summary>
        /// Handles the "getUnityInfo" action from JavaScript.
        /// </summary>
        /// <param name="json">The JSON data from JavaScript (not used in this handler).</param>
        private void HandleGetUnityInfo(string json)
        {
            Debug.Log("[SampleModule] Received getUnityInfo message");
            
            // Create a response with Unity information
            var response = new UnityInfoResponse
            {
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                productName = Application.productName,
                companyName = Application.companyName,
                screenWidth = Screen.width,
                screenHeight = Screen.height
            };
            
            SendMessageToJS("unityInfo", JsonUtility.ToJson(response));
        }
        
        /// <summary>
        /// Calls a JavaScript function to display an alert.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void ShowAlert(string message)
        {
            // First parameter is the action, second is the data
            SendMessageToJS("showAlert", $"{{ \"message\": \"{message}\" }}");
        }
        
        /// <summary>
        /// Represents a hello message from JavaScript.
        /// </summary>
        [System.Serializable]
        private class HelloMessage
        {
            public string name;
        }
        
        /// <summary>
        /// Represents a hello response to JavaScript.
        /// </summary>
        [System.Serializable]
        private class HelloResponse
        {
            public string message;
        }
        
        /// <summary>
        /// Represents Unity information sent to JavaScript.
        /// </summary>
        [System.Serializable]
        private class UnityInfoResponse
        {
            public string unityVersion;
            public string platform;
            public string productName;
            public string companyName;
            public int screenWidth;
            public int screenHeight;
        }
    }
}