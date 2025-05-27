using UnityEngine;
using UnityEditor;
using Energy8.Firebase.Auth.Configuration;

namespace Energy8.Firebase.Auth.Editor
{
    [CustomEditor(typeof(FirebaseAuthConfiguration))]
    public class FirebaseAuthConfigurationEditor : UnityEditor.Editor
    {
        private SerializedProperty enableAutoSignInProp;
        private SerializedProperty persistUserProp;
        private SerializedProperty useEmulatorProp;
        private SerializedProperty emulatorHostProp;
        private SerializedProperty emulatorPortProp;
        private SerializedProperty enabledProvidersProp;

        private void OnEnable()
        {
            enableAutoSignInProp = serializedObject.FindProperty("enableAutoSignIn");
            persistUserProp = serializedObject.FindProperty("persistUser");
            useEmulatorProp = serializedObject.FindProperty("useEmulator");
            emulatorHostProp = serializedObject.FindProperty("emulatorHost");
            emulatorPortProp = serializedObject.FindProperty("emulatorPort");
            enabledProvidersProp = serializedObject.FindProperty("enabledProviders");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Firebase Auth Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // General Settings
            EditorGUILayout.LabelField("General Settings", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(enableAutoSignInProp, new GUIContent("Enable Auto Sign In", "Automatically sign in users on app start"));
            EditorGUILayout.PropertyField(persistUserProp, new GUIContent("Persist User", "Keep user signed in between sessions"));
            EditorGUILayout.Space();

            // Emulator Settings
            EditorGUILayout.LabelField("Emulator Settings", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(useEmulatorProp, new GUIContent("Use Emulator", "Connect to Firebase Auth Emulator"));
            
            if (useEmulatorProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(emulatorHostProp, new GUIContent("Host"));
                EditorGUILayout.PropertyField(emulatorPortProp, new GUIContent("Port"));
                  EditorGUILayout.HelpBox($"Emulator URL: {FirebaseAuthConfiguration.GetEmulatorUrl()}", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            // Provider Settings
            EditorGUILayout.LabelField("Authentication Providers", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(enabledProvidersProp, new GUIContent("Enabled Providers"), true);

            if (GUILayout.Button("Reset to Defaults"))
            {
                ResetToDefaults();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ResetToDefaults()
        {
            if (EditorUtility.DisplayDialog("Reset Configuration", 
                "This will reset all settings to default values. Continue?", "Yes", "No"))
            {
                enableAutoSignInProp.boolValue = false;
                persistUserProp.boolValue = true;
                useEmulatorProp.boolValue = false;
                emulatorHostProp.stringValue = "localhost";
                emulatorPortProp.intValue = 9099;
                
                enabledProvidersProp.ClearArray();
                enabledProvidersProp.InsertArrayElementAtIndex(0);
                enabledProvidersProp.GetArrayElementAtIndex(0).stringValue = "email";
                enabledProvidersProp.InsertArrayElementAtIndex(1);
                enabledProvidersProp.GetArrayElementAtIndex(1).stringValue = "anonymous";
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
