// Sample usage of Firebase Analytics in WebGL
using System.Collections.Generic;
using UnityEngine;
using Energy8.Identity.Core.Analytics.Services;
using Energy8.Identity.Core.Analytics.Providers;

namespace Energy8.Identity.Samples
{
    public class AnalyticsSample : MonoBehaviour
    {
        private IAnalyticsService analyticsService;

        // Example of initializing analytics service manually
        // In real-world scenarios, use dependency injection instead
        void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var analyticsProvider = new WebGLAnalyticsProvider();
#else
            var analyticsProvider = new DefaultAnalyticsProvider();
#endif
            analyticsService = new AnalyticsService(analyticsProvider);
            InitializeAnalytics();
        }

        async void InitializeAnalytics()
        {
            await analyticsService.Initialize(this.GetCancellationTokenOnDestroy());
            
            // Log screen view
            analyticsService.LogScreenView("main_menu");
            
            // Log custom event
            var parameters = new Dictionary<string, object>
            {
                { "level", 1 },
                { "difficulty", "easy" }
            };
            analyticsService.LogEvent("game_started", parameters);
            
            // Set user properties
            var userProps = new Dictionary<string, object>
            {
                { "preferred_language", "en" },
                { "theme", "dark" }
            };
            analyticsService.SetUserProperties(userProps);
        }

        // Example of tracking screen navigation
        public void OnLevelSelect()
        {
            analyticsService.LogScreenView("level_selection");
        }
        
        // Example of tracking user actions
        public void OnButtonClick(string buttonName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "button_name", buttonName },
                { "screen", "main_menu" }
            };
            analyticsService.LogUserAction("button_click", parameters);
        }
        
        // Example of tracking game events
        public void OnLevelComplete(int levelId, int score, float timeSpent)
        {
            var parameters = new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "score", score },
                { "time_spent", timeSpent }
            };
            analyticsService.LogEvent("level_complete", parameters);
        }
    }
}
