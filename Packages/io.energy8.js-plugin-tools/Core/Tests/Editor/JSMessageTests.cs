using System;
using NUnit.Framework;
using UnityEngine;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Tests
{
    /// <summary>
    /// Тесты для проверки функциональности JSMessage
    /// </summary>
    public class JSMessageTests
    {
        [Test]
        public void JSMessage_Serialization_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var originalMessage = new JSMessage
            {
                moduleId = "testModule",
                action = "testAction",
                data = "{\"key\":\"value\"}",
                callbackId = "callback123"
            };
            
            // Act - сериализуем в JSON и обратно
            string jsonString = JsonUtility.ToJson(originalMessage);
            var deserializedMessage = JsonUtility.FromJson<JSMessage>(jsonString);
            
            // Assert - проверяем, что все поля сохранились корректно
            Assert.AreEqual(originalMessage.moduleId, deserializedMessage.moduleId, "Module ID doesn't match after serialization");
            Assert.AreEqual(originalMessage.action, deserializedMessage.action, "Action doesn't match after serialization");
            Assert.AreEqual(originalMessage.data, deserializedMessage.data, "Data doesn't match after serialization");
            Assert.AreEqual(originalMessage.callbackId, deserializedMessage.callbackId, "Callback ID doesn't match after serialization");
        }
        
        [Test]
        public void JSMessage_SerializationWithNullValues_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var originalMessage = new JSMessage
            {
                moduleId = "testModule",
                action = "testAction",
                data = null,
                callbackId = null
            };
            
            // Act
            string jsonString = JsonUtility.ToJson(originalMessage);
            var deserializedMessage = JsonUtility.FromJson<JSMessage>(jsonString);
            
            // Assert
            Assert.AreEqual(originalMessage.moduleId, deserializedMessage.moduleId, "Module ID doesn't match after serialization");
            Assert.AreEqual(originalMessage.action, deserializedMessage.action, "Action doesn't match after serialization");
            Assert.IsNull(deserializedMessage.data, "Data should be null after serialization");
            Assert.IsNull(deserializedMessage.callbackId, "Callback ID should be null after serialization");
        }
        
        [Test]
        public void JSMessage_SerializationWithSpecialCharacters_ShouldHandleSpecialCharactersCorrectly()
        {
            // Arrange - создаем сообщение со специальными символами
            var originalMessage = new JSMessage
            {
                moduleId = "test/Module",
                action = "test\"Action",
                data = "{\"special\":\"Кириллица & <script>\"}", // Включаем кириллицу и HTML-символы
                callbackId = "callback\n\r\t" // Спецсимволы переноса строк и табуляции
            };
            
            // Act
            string jsonString = JsonUtility.ToJson(originalMessage);
            var deserializedMessage = JsonUtility.FromJson<JSMessage>(jsonString);
            
            // Assert
            Assert.AreEqual(originalMessage.moduleId, deserializedMessage.moduleId, "Module ID doesn't match after serialization");
            Assert.AreEqual(originalMessage.action, deserializedMessage.action, "Action doesn't match after serialization");
            Assert.AreEqual(originalMessage.data, deserializedMessage.data, "Data doesn't match after serialization");
            Assert.AreEqual(originalMessage.callbackId, deserializedMessage.callbackId, "Callback ID doesn't match after serialization");
        }
    }
}
