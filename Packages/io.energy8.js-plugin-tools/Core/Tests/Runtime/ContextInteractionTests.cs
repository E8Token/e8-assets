using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки контекстного взаимодействия JS плагинов с Unity
    /// </summary>
    public class ContextInteractionTests
    {
        // Тестовый модуль для работы с GameObject и Transform
        private class UnityObjectsModule : PluginModuleBase, IJSMessageHandler
        {
            private GameObject _testObject;
            public Vector3 LastPosition { get; private set; }
            public List<string> ReceivedActions { get; } = new List<string>();
            
            public UnityObjectsModule() : base("unity_objects_module") { }
            
            protected override bool OnInitialize()
            {
                // Создаем тестовый объект при инициализации
                _testObject = new GameObject("TestObject");
                _testObject.transform.position = Vector3.zero;
                
                return true;
            }
            
            protected override void OnShutdown()
            {
                // Уничтожаем объект при выключении
                if (_testObject != null)
                {
                    Object.Destroy(_testObject);
                    _testObject = null;
                }
                
                ReceivedActions.Clear();
            }
            
            public void HandleJSMessage(JSMessage message)
            {
                ReceivedActions.Add(message.action);
                
                switch (message.action)
                {
                    case "move_object":
                        // Перемещаем объект на указанную позицию
                        if (_testObject != null)
                        {
                            var positionData = JsonUtility.FromJson<PositionData>(message.data);
                            Vector3 newPosition = new Vector3(positionData.x, positionData.y, positionData.z);
                            
                            _testObject.transform.position = newPosition;
                            LastPosition = newPosition;
                            
                            // Отправляем ответ
                            SendMessageToJS("object_moved", JsonUtility.ToJson(positionData), message.callbackId);
                        }
                        break;
                    
                    case "get_position":
                        // Получаем текущую позицию объекта
                        if (_testObject != null)
                        {
                            var position = _testObject.transform.position;
                            var positionData = new PositionData
                            {
                                x = position.x,
                                y = position.y,
                                z = position.z
                            };
                            
                            SendMessageToJS("position_info", JsonUtility.ToJson(positionData), message.callbackId);
                        }
                        break;
                }
            }
            
            // Класс для сериализации данных о позиции
            [System.Serializable]
            public class PositionData
            {
                public float x;
                public float y;
                public float z;
            }
        }
        
        private UnityObjectsModule _testModule;
        private PluginManager _pluginManager;
        private TestJSCallHandler _testCallHandler;
        
        // Тестовый обработчик JS вызовов
        private class TestJSCallHandler : IJSCallHandler
        {
            public List<(string FunctionPath, object[] Args)> Calls { get; } = new List<(string, object[])>();
            
            public void HandleCall(string functionPath, object[] args)
            {
                Calls.Add((functionPath, args));
            }
            
            public void Reset()
            {
                Calls.Clear();
            }
        }
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Создаем тестовый модуль
            _testModule = new UnityObjectsModule();
            
            // Устанавливаем тестовый обработчик JS вызовов
            _testCallHandler = new TestJSCallHandler();
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { _testCallHandler });
            
            // Получаем экземпляр PluginManager
            _pluginManager = PluginManager.Instance;
            
            // Регистрируем и инициализируем тестовый модуль
            _pluginManager.RegisterModule(_testModule);
            _pluginManager.InitializeModule(_testModule.ModuleId);
            
            yield return null;
        }
        
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Отключаем тестовый модуль
            if (_pluginManager != null && _testModule != null)
            {
                _pluginManager.ShutdownModule(_testModule.ModuleId);
            }
            
            // Восстанавливаем стандартный обработчик JS вызовов
            var methodInfo = typeof(ExternalCommunicator).GetMethod("SetCallHandler", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { null });
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator JSMessage_MoveObject_ShouldUpdateTransformPosition()
        {
            // Arrange
            _testCallHandler.Reset();
            var positionData = new UnityObjectsModule.PositionData
            {
                x = 1.0f,
                y = 2.0f,
                z = 3.0f
            };
            
            var message = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "move_object",
                data = JsonUtility.ToJson(positionData),
                callbackId = "move_callback"
            };
            
            // Act
            _pluginManager.OnMessageFromJS(JsonUtility.ToJson(message));
            
            // Ждем один кадр для обработки сообщения
            yield return null;
            
            // Assert
            Assert.AreEqual(1.0f, _testModule.LastPosition.x, 0.001f, "X position should be updated");
            Assert.AreEqual(2.0f, _testModule.LastPosition.y, 0.001f, "Y position should be updated");
            Assert.AreEqual(3.0f, _testModule.LastPosition.z, 0.001f, "Z position should be updated");
            
            // Проверяем, что был отправлен ответ
            Assert.IsTrue(_testCallHandler.Calls.Count > 0, "Response should be sent");
            
            bool responseFound = false;
            foreach (var call in _testCallHandler.Calls)
            {
                if (call.FunctionPath == "UnityWebPlugin.Core.receiveMessageFromUnity")
                {
                    var jsonMessage = call.Args[0] as string;
                    var responseMessage = JsonUtility.FromJson<JSMessage>(jsonMessage);
                    
                    if (responseMessage.action == "object_moved" && responseMessage.callbackId == "move_callback")
                    {
                        responseFound = true;
                        break;
                    }
                }
            }
            
            Assert.IsTrue(responseFound, "Object moved response should be sent");
        }
        
        [UnityTest]
        public IEnumerator JSMessage_GetPosition_ShouldReturnCurrentPosition()
        {
            // Arrange - сначала перемещаем объект в определенную позицию
            var moveData = new UnityObjectsModule.PositionData
            {
                x = 5.0f,
                y = 6.0f,
                z = 7.0f
            };
            
            var moveMessage = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "move_object",
                data = JsonUtility.ToJson(moveData),
                callbackId = null
            };
            
            _pluginManager.OnMessageFromJS(JsonUtility.ToJson(moveMessage));
            yield return null;
            
            // Очищаем историю вызовов
            _testCallHandler.Reset();
            
            // Act - запрашиваем позицию объекта
            var getMessage = new JSMessage
            {
                moduleId = _testModule.ModuleId,
                action = "get_position",
                data = null,
                callbackId = "position_callback"
            };
            
            _pluginManager.OnMessageFromJS(JsonUtility.ToJson(getMessage));
            
            // Ждем один кадр для обработки сообщения
            yield return null;
            
            // Assert
            Assert.IsTrue(_testCallHandler.Calls.Count > 0, "Response should be sent");
            
            UnityObjectsModule.PositionData responsePosition = null;
            foreach (var call in _testCallHandler.Calls)
            {
                if (call.FunctionPath == "UnityWebPlugin.Core.receiveMessageFromUnity")
                {
                    var jsonMessage = call.Args[0] as string;
                    var responseMessage = JsonUtility.FromJson<JSMessage>(jsonMessage);
                    
                    if (responseMessage.action == "position_info" && responseMessage.callbackId == "position_callback")
                    {
                        responsePosition = JsonUtility.FromJson<UnityObjectsModule.PositionData>(responseMessage.data);
                        break;
                    }
                }
            }
            
            Assert.IsNotNull(responsePosition, "Position info should be returned");
            Assert.AreEqual(5.0f, responsePosition.x, 0.001f, "X position should match");
            Assert.AreEqual(6.0f, responsePosition.y, 0.001f, "Y position should match");
            Assert.AreEqual(7.0f, responsePosition.z, 0.001f, "Z position should match");
        }
        
        [UnityTest]
        public IEnumerator JSMessage_MultipleMessages_ShouldHandleInOrder()
        {
            // Arrange
            _testModule.ReceivedActions.Clear();
            
            var actions = new string[] { "action1", "action2", "action3", "action4", "action5" };
            
            // Act - отправляем несколько сообщений подряд
            foreach (var action in actions)
            {
                var message = new JSMessage
                {
                    moduleId = _testModule.ModuleId,
                    action = action,
                    data = null,
                    callbackId = null
                };
                
                _pluginManager.OnMessageFromJS(JsonUtility.ToJson(message));
            }
            
            // Ждем несколько кадров для обработки всех сообщений
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            Assert.AreEqual(actions.Length, _testModule.ReceivedActions.Count, "All messages should be processed");
            
            // Проверяем порядок обработки сообщений
            for (int i = 0; i < actions.Length; i++)
            {
                Assert.AreEqual(actions[i], _testModule.ReceivedActions[i], $"Message {i} should be processed in order");
            }
        }
    }
}
