#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Energy8.BuildSystem.Utils
{
    public class BuildLogger
    {
        private string logFilePath;
        private StreamWriter logWriter;
        
        public BuildLogger(string buildPath, string platformName)
        {
            InitializeLog(buildPath, platformName);
        }
        
        private void InitializeLog(string buildPath, string platformName)
        {
            string version = PlayerSettings.bundleVersion;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string logFileName = $"BuildLog_{platformName}_v{version}_{timestamp}.txt";

            string logsDirectory = Path.Combine(buildPath, "../BuildLogs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            logFilePath = Path.Combine(logsDirectory, logFileName);
            logWriter = new StreamWriter(logFilePath, false);
            
            logWriter.WriteLine($"Build log started at {DateTime.Now}");
            logWriter.WriteLine($"===============================================");
            logWriter.Flush();
            
            // Subscribe to Unity logs
            Application.logMessageReceived += HandleUnityLog;
        }
        
        public void Log(string message)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {message}");
                logWriter.Flush();
            }
        }
        
        public void LogError(string message, Exception exception = null)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}][ERROR] {message}");
                if (exception != null)
                {
                    logWriter.WriteLine(exception.StackTrace);
                }
                logWriter.Flush();
            }
        }
        
        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}][{type}] {logString}");
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
                {
                    logWriter.WriteLine(stackTrace);
                }
                logWriter.Flush();
            }
        }
        
        public void Close(DateTime buildStartTime)
        {
            // Unsubscribe from Unity logs
            Application.logMessageReceived -= HandleUnityLog;
            
            if (logWriter != null)
            {
                DateTime buildEndTime = DateTime.Now;
                TimeSpan buildDuration = buildEndTime - buildStartTime;
                
                logWriter.WriteLine($"Build completed at {buildEndTime.ToLongTimeString()}");
                logWriter.WriteLine($"Build duration: {buildDuration.Hours}:{buildDuration.Minutes}:{buildDuration.Seconds}");
                logWriter.WriteLine($"===============================================");
                logWriter.Close();
                logWriter = null;
            }
        }
        
        public string GetLogPath()
        {
            return logFilePath;
        }
    }
}
#endif