using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;
using System.Collections;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки функциональности ExternalCommunicator
    /// </summary>
    public class ExternalCommunicatorTests
    {
        // Тестовый обработчик JS вызовов для проверки функциональности
        private class TestJSCallHandler : IJSCallHandler
        {
            public string LastFunctionPath { get; private set; }
            public object[] LastArgs { get; private set; }
            public bool WasCalled { get; private set; }

            public void HandleCall(string functionPath, object[] args)
            {
                LastFunctionPath = functionPath;
                LastArgs = args;
                WasCalled = true;
            }

            public void Reset()
            {
                LastFunctionPath = null;
                LastArgs = null;
                WasCalled = false;
            }
        }

        private TestJSCallHandler _testHandler;

        [SetUp]
        public void SetUp()
        {
            _testHandler = new TestJSCallHandler();
            // Используем рефлексию для доступа к приватному методу SetCallHandler
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { _testHandler });
        }

        [TearDown]
        public void TearDown()
        {
            // Восстанавливаем стандартный обработчик после теста
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { null });
        }

        [Test]
        public void CallJS_WithValidFunction_ShouldPassCorrectParameters()
        {
            // Arrange
            string functionPath = "UnityWebPlugin.Core.initialize";
            
            // Act
            ExternalCommunicator.CallJS(functionPath);
            
            // Assert
            Assert.IsTrue(_testHandler.WasCalled, "Handler was not called");
            Assert.AreEqual(functionPath, _testHandler.LastFunctionPath, "Function path doesn't match");
            Assert.IsNotNull(_testHandler.LastArgs, "Args should not be null");
            Assert.AreEqual(0, _testHandler.LastArgs.Length, "Args should be empty array");
        }
        
        [Test]
        public void CallJS_WithArguments_ShouldPassArgumentsCorrectly()
        {
            // Arrange
            string functionPath = "UnityWebPlugin.Core.someFunction";
            object[] args = new object[] { "arg1", 123, true };
            
            // Act
            ExternalCommunicator.CallJS(functionPath, args);
            
            // Assert
            Assert.IsTrue(_testHandler.WasCalled, "Handler was not called");
            Assert.AreEqual(functionPath, _testHandler.LastFunctionPath, "Function path doesn't match");
            Assert.IsNotNull(_testHandler.LastArgs, "Args should not be null");
            
            // Проверяем, что аргументы передаются корректно
            Assert.AreEqual(1, _testHandler.LastArgs.Length, "Args should contain one element (the array)");
            Assert.IsTrue(_testHandler.LastArgs[0] is object[], "First arg should be an object array");
            var passedArgs = _testHandler.LastArgs[0] as object[];
            Assert.AreEqual(3, passedArgs.Length, "Passed args should have 3 elements");
            Assert.AreEqual("arg1", passedArgs[0], "First argument doesn't match");
            Assert.AreEqual(123, passedArgs[1], "Second argument doesn't match");
            Assert.AreEqual(true, passedArgs[2], "Third argument doesn't match");
        }

        [Test]
        public void SendMessageToJS_ShouldCreateCorrectJSMessage()
        {
            // Arrange
            string moduleId = "testModule";
            string action = "testAction";
            string data = "{\"key\":\"value\"}";
            string callbackId = "callback123";
            
            // Act
            ExternalCommunicator.SendMessageToJS(moduleId, action, data, callbackId);
            
            // Assert
            Assert.IsTrue(_testHandler.WasCalled, "Handler was not called");
            Assert.AreEqual("UnityWebPlugin.Core.receiveMessageFromUnity", _testHandler.LastFunctionPath, 
                "Function path doesn't match");
            
            // Проверяем, что сообщение передаётся корректно
            Assert.IsNotNull(_testHandler.LastArgs, "Args should not be null");
            Assert.AreEqual(1, _testHandler.LastArgs.Length, "Args should have 1 element");
            Assert.IsTrue(_testHandler.LastArgs[0] is string, "First arg should be a string (JSON)");
            
            // Десериализуем сообщение и проверяем его содержимое
            var message = JsonUtility.FromJson<JSMessage>(_testHandler.LastArgs[0] as string);
            Assert.AreEqual(moduleId, message.moduleId, "Module ID doesn't match");
            Assert.AreEqual(action, message.action, "Action doesn't match");
            Assert.AreEqual(data, message.data, "Data doesn't match");
            Assert.AreEqual(callbackId, message.callbackId, "Callback ID doesn't match");
        }
        
        [Test]
        public void SendMessageToJS_WithNullData_ShouldHandleNullData()
        {
            // Arrange
            string moduleId = "testModule";
            string action = "testAction";
            
            // Act
            ExternalCommunicator.SendMessageToJS(moduleId, action);
            
            // Assert
            Assert.IsTrue(_testHandler.WasCalled, "Handler was not called");
            
            // Десериализуем сообщение и проверяем его содержимое
            var message = JsonUtility.FromJson<JSMessage>(_testHandler.LastArgs[0] as string);
            Assert.AreEqual(moduleId, message.moduleId, "Module ID doesn't match");
            Assert.AreEqual(action, message.action, "Action doesn't match");
            Assert.IsNull(message.data, "Data should be null");
            Assert.IsNull(message.callbackId, "Callback ID should be null");
        }
    }
}
