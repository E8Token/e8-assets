using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Energy8.JSPluginTools.Core;
using UnityEngine.TestTools;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки внутренних обработчиков JS вызовов (WebGLJSCallHandler и EditorJSCallHandler)
    /// </summary>
    public class JSCallHandlersTests
    {
        // Получение внутренних классов через рефлексию
        private Type GetWebGLHandlerType()
        {
            return typeof(ExternalCommunicator).GetNestedType("WebGLJSCallHandler", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        private Type GetEditorHandlerType()
        {
            return typeof(ExternalCommunicator).GetNestedType("EditorJSCallHandler", 
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        [Test]
        public void WebGLJSCallHandler_ShouldImplementIJSCallHandler()
        {
            // Act
            Type webGLHandlerType = GetWebGLHandlerType();
            
            // Assert
            Assert.IsNotNull(webGLHandlerType, "WebGLJSCallHandler type should exist");
            Assert.IsTrue(typeof(IJSCallHandler).IsAssignableFrom(webGLHandlerType), 
                "WebGLJSCallHandler should implement IJSCallHandler");
        }
        
        [Test]
        public void EditorJSCallHandler_ShouldImplementIJSCallHandler()
        {
            // Act
            Type editorHandlerType = GetEditorHandlerType();
            
            // Assert
            Assert.IsNotNull(editorHandlerType, "EditorJSCallHandler type should exist");
            Assert.IsTrue(typeof(IJSCallHandler).IsAssignableFrom(editorHandlerType), 
                "EditorJSCallHandler should implement IJSCallHandler");
        }
        
        [Test]
        public void EditorJSCallHandler_HandleCall_ShouldNotThrowException()
        {
            // Arrange
            Type editorHandlerType = GetEditorHandlerType();
            var editorHandler = Activator.CreateInstance(editorHandlerType) as IJSCallHandler;
            
            // Act & Assert - просто проверяем, что не будет исключений
            Assert.DoesNotThrow(() => editorHandler.HandleCall("testFunction", new object[] { 1, "test", true }));
        }
        
        // Примечание: Тесты для WebGLJSCallHandler невозможно полноценно выполнить в редакторе,
        // поскольку он использует Application.ExternalCall, который работает только в WebGL сборке.
        // Поэтому мы проверяем только структуру и наличие методов.
        
        [Test]
        public void CreateDefaultHandler_ShouldReturnCorrectHandlerType()
        {
            // Используем рефлексию для доступа к приватному методу
            MethodInfo createDefaultHandlerMethod = typeof(ExternalCommunicator).GetMethod("CreateDefaultHandler", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            // Act
            var handler = createDefaultHandlerMethod.Invoke(null, null) as IJSCallHandler;
            
            // Assert
            #if UNITY_WEBGL && !UNITY_EDITOR
            // В WebGL сборке должен возвращаться WebGLJSCallHandler
            Assert.IsInstanceOf(GetWebGLHandlerType(), handler, 
                "In WebGL build, CreateDefaultHandler should return WebGLJSCallHandler");
            #else
            // В других сборках должен возвращаться EditorJSCallHandler
            Assert.IsInstanceOf(GetEditorHandlerType(), handler, 
                "In non-WebGL builds, CreateDefaultHandler should return EditorJSCallHandler");
            #endif
        }
        
        [Test]
        public void SetCallHandler_WithNull_ShouldResetToDefaultHandler()
        {
            // Arrange - сохраняем текущий обработчик
            IJSCallHandler originalHandler = null;
            
            // Используем рефлексию, чтобы получить текущий обработчик
            FieldInfo callHandlerField = typeof(ExternalCommunicator).GetField("_callHandler", 
                BindingFlags.NonPublic | BindingFlags.Static);
            originalHandler = callHandlerField.GetValue(null) as IJSCallHandler;
            
            // Создаем тестовый обработчик
            var testHandler = new TestJSCallHandler();
            
            try
            {
                // Act - устанавливаем тестовый обработчик
                MethodInfo setCallHandlerMethod = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                    BindingFlags.NonPublic | BindingFlags.Static);
                setCallHandlerMethod.Invoke(null, new object[] { testHandler });
                
                // Проверяем, что обработчик установлен
                var currentHandler = callHandlerField.GetValue(null);
                Assert.AreEqual(testHandler, currentHandler, "Test handler should be set");
                
                // Act - сбрасываем обработчик к стандартному
                setCallHandlerMethod.Invoke(null, new object[] { null });
                
                // Assert - проверяем, что обработчик сброшен к стандартному
                var resetHandler = callHandlerField.GetValue(null);
                Assert.IsNotNull(resetHandler, "Handler should not be null after reset");
                Assert.AreNotEqual(testHandler, resetHandler, "Handler should not be test handler after reset");
                
                // Проверяем тип стандартного обработчика
                #if UNITY_WEBGL && !UNITY_EDITOR
                Assert.IsInstanceOf(GetWebGLHandlerType(), resetHandler, 
                    "Reset handler should be WebGLJSCallHandler in WebGL build");
                #else
                Assert.IsInstanceOf(GetEditorHandlerType(), resetHandler, 
                    "Reset handler should be EditorJSCallHandler in non-WebGL build");
                #endif
            }
            finally
            {
                // Восстанавливаем исходный обработчик
                callHandlerField.SetValue(null, originalHandler);
            }
        }
        
        // Вспомогательный класс для тестирования
        private class TestJSCallHandler : IJSCallHandler
        {
            public void HandleCall(string functionPath, object[] args)
            {
                // Пустая реализация для тестирования
            }
        }
    }
}
