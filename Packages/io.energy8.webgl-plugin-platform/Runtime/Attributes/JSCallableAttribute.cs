using System;

namespace Energy8.WebGL.PluginPlatform
{
    /// <summary>
    /// Атрибут для маркировки методов, доступных из JavaScript
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JSCallableAttribute : Attribute
    {
        /// <summary>
        /// Название метода для вызова из JavaScript (если отличается от имени метода)
        /// </summary>
        public string MethodName { get; set; }
        
        /// <summary>
        /// Описание метода
        /// </summary>
        public string Description { get; set; }
        
        public JSCallableAttribute()
        {
        }
        
        public JSCallableAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
