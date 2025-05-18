using System;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Категории тестов для модулей JSPluginTools
    /// </summary>
    public static class TestCategories
    {
        /// <summary>
        /// Базовые тесты Unit, тестирующие API и обмен сообщениями
        /// </summary>
        public const string Core = "JSPluginTools.Core";
        
        /// <summary>
        /// Тесты интеграции с Unity компонентами
        /// </summary>
        public const string Integration = "JSPluginTools.Integration";
        
        /// <summary>
        /// Тесты для WebGL платформы
        /// </summary>
        public const string WebGL = "JSPluginTools.WebGL";
        
        /// <summary>
        /// Тесты для проверки обработки ошибок
        /// </summary>
        public const string ErrorHandling = "JSPluginTools.ErrorHandling";
        
        /// <summary>
        /// Тесты производительности
        /// </summary>
        public const string Performance = "JSPluginTools.Performance";
    }
}
