using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public class DeployMonitorWindow : EditorWindow, IDeployMonitor
    {
        private Vector2 scrollPosition;
        private List<DeployLogEntry> deployLogs = new List<DeployLogEntry>();
        private bool isDeploying = false;
        private string currentDeployTarget = "";
        private float deployProgress = 0f;
        
        private struct DeployLogEntry
        {
            public DateTime timestamp;
            public string message;
            public LogType type;
            
            public DeployLogEntry(string message, LogType type = LogType.Log)
            {
                this.timestamp = DateTime.Now;
                this.message = message;
                this.type = type;
            }
        }
        
        public static void ShowWindow()
        {
            var window = GetWindow<DeployMonitorWindow>("Deploy Monitor");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }
        
        public static DeployMonitorWindow GetInstance()
        {
            return GetWindow<DeployMonitorWindow>("Deploy Monitor");
        }
        
        public void StartDeployMonitoring(string target)
        {
            isDeploying = true;
            currentDeployTarget = target;
            deployProgress = 0f;
            deployLogs.Clear();
            AddLog($"🚀 Starting deployment to {target}", LogType.Log);
            Repaint();
        }
          public void UpdateDeployProgress(float progress, string message = null)
        {
            deployProgress = Mathf.Clamp01(progress);
            if (!string.IsNullOrEmpty(message))
            {
                AddLog(message, LogType.Log);
            }
            Repaint();
        }
        
        public void AddLog(string message, LogType type = LogType.Log)
        {
            deployLogs.Add(new DeployLogEntry(message, type));
            
            // Ограничиваем количество логов
            if (deployLogs.Count > 100)
            {
                deployLogs.RemoveAt(0);
            }
            
            Repaint();
        }
        
        public void CompleteDeployment(bool success)
        {
            isDeploying = false;
            deployProgress = success ? 1f : 0f;
            
            if (success)
            {
                AddLog("✅ Deployment completed successfully!", LogType.Log);
            }
            else
            {
                AddLog("❌ Deployment failed!", LogType.Error);
            }
            
            Repaint();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(10);
            
            DrawProgressSection();
            EditorGUILayout.Space(10);
            
            DrawLogsSection();
        }
        
        private void DrawHeader()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Deploy Monitor", style);
            
            if (!string.IsNullOrEmpty(currentDeployTarget))
            {
                var targetStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Italic
                };
                
                EditorGUILayout.LabelField($"Target: {currentDeployTarget}", targetStyle);
            }
        }
        
        private void DrawProgressSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            if (isDeploying)
            {
                EditorGUILayout.LabelField("🔄 Deployment in Progress", EditorStyles.boldLabel);
                
                // Progress bar
                var rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, deployProgress, $"{deployProgress * 100:F0}%");
            }
            else
            {
                if (deployProgress >= 1f)
                {
                    EditorGUILayout.LabelField("✅ Last Deployment: Success", EditorStyles.boldLabel);
                }
                else if (deployLogs.Count > 0)
                {
                    EditorGUILayout.LabelField("❌ Last Deployment: Failed", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⏳ Ready for Deployment", EditorStyles.boldLabel);
                }
            }
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🗑️ Clear Logs"))
            {
                deployLogs.Clear();
                Repaint();
            }
            
            if (GUILayout.Button("📋 Copy Logs"))
            {
                CopyLogsToClipboard();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLogsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📜 Deploy Logs", EditorStyles.boldLabel);
            
            if (deployLogs.Count == 0)
            {
                EditorGUILayout.HelpBox("No deployment logs yet.", MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var log in deployLogs)
                {
                    DrawLogEntry(log);
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLogEntry(DeployLogEntry log)
        {
            var color = GetLogColor(log.type);
            var originalColor = GUI.color;
            GUI.color = color;
            
            EditorGUILayout.BeginHorizontal("box");
            GUI.color = originalColor;
            
            // Timestamp
            var timeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                normal = { textColor = Color.gray }
            };
            EditorGUILayout.LabelField(log.timestamp.ToString("HH:mm:ss"), timeStyle, GUILayout.Width(60));
            
            // Icon based on log type
            string icon = GetLogIcon(log.type);
            EditorGUILayout.LabelField(icon, GUILayout.Width(20));
            
            // Message
            var messageStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                normal = { textColor = GetLogTextColor(log.type) }
            };
            EditorGUILayout.LabelField(log.message, messageStyle);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private Color GetLogColor(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    return new Color(1f, 0.8f, 0.8f, 0.3f);
                case LogType.Warning:
                    return new Color(1f, 1f, 0.8f, 0.3f);
                default:
                    return Color.clear;
            }
        }
        
        private Color GetLogTextColor(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    return Color.red;
                case LogType.Warning:
                    return new Color(1f, 0.6f, 0f); // Orange
                default:
                    return GUI.skin.label.normal.textColor;
            }
        }
        
        private string GetLogIcon(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    return "❌";
                case LogType.Warning:
                    return "⚠️";
                default:
                    return "ℹ️";
            }
        }
        
        private void CopyLogsToClipboard()
        {
            var text = string.Join("\n", deployLogs.ConvertAll(log => 
                $"[{log.timestamp:HH:mm:ss}] {log.message}"));
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("[DeployMonitor] Logs copied to clipboard");
        }
    }
}
