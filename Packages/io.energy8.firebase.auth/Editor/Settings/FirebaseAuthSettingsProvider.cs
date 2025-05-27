using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Energy8.Firebase.Auth.Configuration;
using Energy8.Firebase.Auth.Editor;

namespace Energy8.Firebase.Auth.Editor.Settings
{
    public class FirebaseAuthSettingsProvider : SettingsProvider
    {
        private SerializedObject serializedObject;
        private FirebaseAuthConfiguration configuration;

        public FirebaseAuthSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateFirebaseAuthSettingsProvider()
        {
            var provider = new FirebaseAuthSettingsProvider("Project/Firebase/Auth", SettingsScope.Project,
                GetSearchKeywordsFromGUIContentProperties<FirebaseAuthSettingsProvider>());
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            configuration = FirebaseAuthConfiguration.Instance;
            if (configuration != null)
            {
                serializedObject = new SerializedObject(configuration);
            }
        }

        public override void OnGUI(string searchContext)
        {
            if (configuration == null)
            {
                EditorGUILayout.HelpBox("Firebase Auth Configuration not found.", MessageType.Warning);
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

            EditorGUILayout.LabelField("Firebase Auth Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // General Settings
            DrawGeneralSettings();
            EditorGUILayout.Space();

            // Emulator Settings
            DrawEmulatorSettings();
            EditorGUILayout.Space();

            // Provider Settings
            DrawProviderSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            
            FirebaseAuthConfiguration.EnableAutoSignIn = EditorGUILayout.Toggle("Enable Auto Sign In", FirebaseAuthConfiguration.EnableAutoSignIn);
            FirebaseAuthConfiguration.PersistUser = EditorGUILayout.Toggle("Persist User", FirebaseAuthConfiguration.PersistUser);
        }

        private void DrawEmulatorSettings()
        {
            EditorGUILayout.LabelField("Emulator Settings", EditorStyles.boldLabel);
            
            FirebaseAuthConfiguration.UseEmulator = EditorGUILayout.Toggle("Use Emulator", FirebaseAuthConfiguration.UseEmulator);
            
            if (FirebaseAuthConfiguration.UseEmulator)
            {
                EditorGUI.indentLevel++;
                FirebaseAuthConfiguration.EmulatorHost = EditorGUILayout.TextField("Emulator Host", FirebaseAuthConfiguration.EmulatorHost);
                FirebaseAuthConfiguration.EmulatorPort = EditorGUILayout.IntField("Emulator Port", FirebaseAuthConfiguration.EmulatorPort);
                
                EditorGUILayout.HelpBox($"Emulator URL: {FirebaseAuthConfiguration.GetEmulatorUrl()}", MessageType.Info);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawProviderSettings()
        {
            EditorGUILayout.LabelField("Authentication Providers", EditorStyles.boldLabel);
            
            var enabledProviders = FirebaseAuthConfiguration.EnabledProviders;
            var availableProviders = new[] { "email", "anonymous", "google", "facebook", "twitter", "github" };
            
            foreach (var provider in availableProviders)
            {
                bool isEnabled = enabledProviders.Contains(provider);
                bool newEnabled = EditorGUILayout.Toggle(provider.ToUpper(), isEnabled);
                
                if (newEnabled != isEnabled)
                {
                    if (newEnabled)
                    {
                        enabledProviders.Add(provider);
                    }
                    else
                    {
                        enabledProviders.Remove(provider);
                    }
                    
                    EditorUtility.SetDirty(configuration);
                }
            }
        }

        public override void OnDeactivate()
        {
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static FirebaseAuthConfiguration CreateConfigurationFile()
        {
            return FirebaseAuthEditorUtilities.CreateConfigurationFile();
        }
    }
}
