using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Energy8.Firebase.Analytics.Configuration;
using Energy8.Firebase.Analytics.Editor;

namespace Energy8.Firebase.Analytics.Editor.Settings
{
    public class FirebaseAnalyticsSettingsProvider : SettingsProvider
    {
        private SerializedObject serializedObject;
        private FirebaseAnalyticsConfiguration configuration;

        public FirebaseAnalyticsSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateFirebaseAnalyticsSettingsProvider()
        {
            var provider = new FirebaseAnalyticsSettingsProvider("Project/Firebase/Analytics", SettingsScope.Project,
                GetSearchKeywordsFromGUIContentProperties<FirebaseAnalyticsSettingsProvider>());
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            configuration = FirebaseAnalyticsConfiguration.Instance;
            if (configuration != null)
            {
                serializedObject = new SerializedObject(configuration);
            }
        }

        public override void OnGUI(string searchContext)
        {
            if (configuration == null)
            {
                EditorGUILayout.HelpBox("Firebase Analytics Configuration not found.", MessageType.Warning);
                if (GUILayout.Button("Create Configuration"))
                {
                    configuration = CreateConfigurationFile();
                    if (configuration != null)
                    {
                        serializedObject = new SerializedObject(configuration);
                    }
                }
                return;
            }

            serializedObject.Update();

            EditorGUILayout.LabelField("Firebase Analytics Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // General Settings
            DrawGeneralSettings();
            EditorGUILayout.Space();

            // Advanced Settings
            DrawAdvancedSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            // Enable Analytics
            var enableAnalytics = serializedObject.FindProperty("enableAnalytics");
            EditorGUILayout.PropertyField(enableAnalytics, new GUIContent("Enable Analytics", "Enable or disable Firebase Analytics collection"));
            
            // Enable Debug Logging
            var enableDebugLogging = serializedObject.FindProperty("enableDebugLogging");
            EditorGUILayout.PropertyField(enableDebugLogging, new GUIContent("Enable Debug Logging", "Enable debug logging for analytics events"));
            
            // Enable Automatic Screen Reporting
            var enableAutomaticScreenReporting = serializedObject.FindProperty("enableAutomaticScreenReporting");
            EditorGUILayout.PropertyField(enableAutomaticScreenReporting, new GUIContent("Automatic Screen Reporting", "Automatically report screen views"));
            
            EditorGUI.indentLevel--;
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            // Session Timeout Duration
            var sessionTimeoutDuration = serializedObject.FindProperty("sessionTimeoutDuration");
            var currentValue = sessionTimeoutDuration.longValue;
            var minutes = currentValue / 60000;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Session Timeout (minutes)");
            var newMinutes = EditorGUILayout.LongField(minutes);
            if (newMinutes != minutes)
            {
                sessionTimeoutDuration.longValue = newMinutes * 60000;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox($"Current timeout: {newMinutes} minutes ({sessionTimeoutDuration.longValue} milliseconds)", MessageType.Info);
            
            // Custom Parameters
            var customParameters = serializedObject.FindProperty("customParameters");
            EditorGUILayout.PropertyField(customParameters, new GUIContent("Custom Parameters", "Custom parameter names for your analytics events"), true);
            
            EditorGUI.indentLevel--;
        }

        public override void OnDeactivate()
        {
            if (serializedObject != null)
            {
                serializedObject.Dispose();
                serializedObject = null;
            }
        }

        private static FirebaseAnalyticsConfiguration CreateConfigurationFile()
        {
            return FirebaseAnalyticsEditorUtilities.CreateConfigurationFile();
        }
    }
}
