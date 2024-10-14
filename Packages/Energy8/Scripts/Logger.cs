using System;
using UnityEngine;

namespace Energy8
{
    public class Logger
    {
        string className;
        Color headerColor;

        UnityEngine.Object contextObject;

        public Logger(UnityEngine.Object contextObject, string className, Color headerColor)
        {
            this.contextObject = contextObject;
            this.className = className;
            this.headerColor = headerColor;
        }

        public void Log(string message) => Debug.Log(FormatMessage(message), contextObject);
        public void LogWarning(string message) => Debug.LogWarning(FormatMessage(message), contextObject);
        public void LogError(string message) => Debug.LogError(FormatMessage(message), contextObject);

        string FormatMessage(string message)
        {
#if UNITY_EDITOR
        return $"<b><color=#{ColorUtility.ToHtmlStringRGBA(headerColor)}>{className}.</color></b>{message}";
#else
            return $"[{DateTime.UtcNow.ToLongTimeString()}] {className}.{message}";
#endif
        }
    }
}