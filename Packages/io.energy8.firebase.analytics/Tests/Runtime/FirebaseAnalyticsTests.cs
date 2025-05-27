using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Energy8.Firebase.Analytics;
using Energy8.Firebase.Analytics.Models;

namespace Energy8.Firebase.Analytics.Tests
{
    public class FirebaseAnalyticsTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset Firebase Analytics state before each test
            FirebaseAnalytics.ResetForTesting();
        }        [Test]
        public void TestAnalyticsEventCreation()
        {
            // Test basic event creation
            var eventData = new AnalyticsEvent(AnalyticsEventNames.SELECT_CONTENT);
            Assert.IsNotNull(eventData);
            Assert.AreEqual(AnalyticsEventNames.SELECT_CONTENT, eventData.Name);
            Assert.IsNotNull(eventData.Parameters);
            Assert.AreEqual(0, eventData.Parameters.Count);
        }

        [Test]
        public void TestAnalyticsEventWithParameters()
        {
            // Test event with parameters
            var parameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.ITEM_ID, "test_item" },
                { AnalyticsParameterNames.ITEM_NAME, "Test Item" },
                { AnalyticsParameterNames.CONTENT_TYPE, "product" }
            };            var eventData = new AnalyticsEvent(AnalyticsEventNames.SELECT_CONTENT, parameters);
            Assert.IsNotNull(eventData);
            Assert.AreEqual(AnalyticsEventNames.SELECT_CONTENT, eventData.Name);
            Assert.AreEqual(3, eventData.Parameters.Count);
            Assert.AreEqual("test_item", eventData.Parameters[AnalyticsParameterNames.ITEM_ID]);
        }

        [Test]
        public void TestEventNameValidation()
        {
            // Test valid event name
            Assert.IsTrue(AnalyticsEvent.IsValidEventName("test_event"));
            Assert.IsTrue(AnalyticsEvent.IsValidEventName("firebase_event"));
            
            // Test invalid event names
            Assert.IsFalse(AnalyticsEvent.IsValidEventName(""));
            Assert.IsFalse(AnalyticsEvent.IsValidEventName(null));
            Assert.IsFalse(AnalyticsEvent.IsValidEventName("event_with_very_long_name_that_exceeds_forty_characters"));
            Assert.IsFalse(AnalyticsEvent.IsValidEventName("123_invalid_start"));
            Assert.IsFalse(AnalyticsEvent.IsValidEventName("invalid-dash"));
        }

        [Test]
        public void TestParameterNameValidation()
        {
            // Test valid parameter names
            Assert.IsTrue(AnalyticsEvent.IsValidParameterName("item_id"));
            Assert.IsTrue(AnalyticsEvent.IsValidParameterName("custom_param"));
            
            // Test invalid parameter names
            Assert.IsFalse(AnalyticsEvent.IsValidParameterName(""));
            Assert.IsFalse(AnalyticsEvent.IsValidParameterName(null));
            Assert.IsFalse(AnalyticsEvent.IsValidParameterName("param_with_very_long_name_that_exceeds_forty_characters"));
            Assert.IsFalse(AnalyticsEvent.IsValidParameterName("123_invalid"));
            Assert.IsFalse(AnalyticsEvent.IsValidParameterName("invalid-dash"));
        }

        [Test]
        public void TestParameterLimits()
        {
            // Test parameter count limit
            var parameters = new Dictionary<string, object>();
            for (int i = 0; i < AnalyticsEvent.MaxParametersPerEvent + 5; i++)
            {
                parameters[$"param_{i}"] = $"value_{i}";
            }

            var eventData = new AnalyticsEvent("test_event", parameters);
            Assert.LessOrEqual(eventData.Parameters.Count, AnalyticsEvent.MaxParametersPerEvent);
        }

        [Test]
        public void TestStringParameterLengthLimit()
        {
            // Test string parameter length limit
            var longString = new string('a', AnalyticsEvent.MaxStringParameterLength + 10);
            var parameters = new Dictionary<string, object>
            {
                { "long_param", longString }
            };

            var eventData = new AnalyticsEvent("test_event", parameters);
            var actualValue = eventData.Parameters["long_param"] as string;
            Assert.IsNotNull(actualValue);
            Assert.LessOrEqual(actualValue.Length, AnalyticsEvent.MaxStringParameterLength);
        }

        [Test]
        public void TestPredefinedEventNames()
        {
            // Test that predefined event names are not null or empty
            Assert.IsNotNull(AnalyticsEventNames.SELECT_CONTENT);
            Assert.IsNotNull(AnalyticsEventNames.VIEW_ITEM);
            Assert.IsNotNull(AnalyticsEventNames.ADD_TO_CART);
            Assert.IsNotNull(AnalyticsEventNames.PURCHASE);
            Assert.IsNotNull(AnalyticsEventNames.LOGIN);
            Assert.IsNotNull(AnalyticsEventNames.LEVEL_START);
            Assert.IsNotNull(AnalyticsEventNames.LEVEL_END);
            
            Assert.IsNotEmpty(AnalyticsEventNames.SELECT_CONTENT);
            Assert.IsNotEmpty(AnalyticsEventNames.VIEW_ITEM);
            Assert.IsNotEmpty(AnalyticsEventNames.ADD_TO_CART);
        }

        [Test]
        public void TestPredefinedParameterNames()
        {
            // Test that predefined parameter names are not null or empty
            Assert.IsNotNull(AnalyticsParameterNames.ITEM_ID);
            Assert.IsNotNull(AnalyticsParameterNames.ITEM_NAME);
            Assert.IsNotNull(AnalyticsParameterNames.CONTENT_TYPE);
            Assert.IsNotNull(AnalyticsParameterNames.VALUE);
            Assert.IsNotNull(AnalyticsParameterNames.CURRENCY);
            Assert.IsNotNull(AnalyticsParameterNames.LEVEL);
            Assert.IsNotNull(AnalyticsParameterNames.SCORE);
            
            Assert.IsNotEmpty(AnalyticsParameterNames.ITEM_ID);
            Assert.IsNotEmpty(AnalyticsParameterNames.ITEM_NAME);
            Assert.IsNotEmpty(AnalyticsParameterNames.CONTENT_TYPE);
        }        [UnityTest]
        public IEnumerator TestFirebaseAnalyticsInitialization()
        {
            // Test Firebase Analytics initialization
            FirebaseAnalytics.OnInitialized += () => { /* Event fired */ };

            // Initialize Firebase Analytics
            var initTask = FirebaseAnalytics.InitializeAsync();
            
            // Wait for initialization with timeout
            float timeout = 5.0f;
            float elapsed = 0f;
            
            while (!initTask.IsCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(initTask.IsCompleted, "Firebase Analytics should initialize within timeout");
            Assert.IsTrue(FirebaseAnalytics.IsInitialized, "Firebase Analytics should be marked as initialized");
        }

        [UnityTest]
        public IEnumerator TestLogEvent()
        {
            // Ensure Firebase Analytics is initialized
            if (!FirebaseAnalytics.IsInitialized)
            {
                yield return TestFirebaseAnalyticsInitialization();
            }            // Test logging an event
            var eventData = new AnalyticsEvent(AnalyticsEventNames.SELECT_CONTENT, new Dictionary<string, object>
            {
                { AnalyticsParameterNames.ITEM_ID, "test_item" },
                { AnalyticsParameterNames.CONTENT_TYPE, "test" }
            });

            var logTask = FirebaseAnalytics.LogEventAsync(eventData);
            
            // Wait for event logging with timeout
            float timeout = 3.0f;
            float elapsed = 0f;
            
            while (!logTask.IsCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(logTask.IsCompleted, "Event logging should complete within timeout");
            Assert.IsTrue(logTask.Result, "Event should be logged successfully");
        }

        [UnityTest]
        public IEnumerator TestSetUserProperty()
        {
            // Ensure Firebase Analytics is initialized
            if (!FirebaseAnalytics.IsInitialized)
            {
                yield return TestFirebaseAnalyticsInitialization();
            }

            // Test setting user property
            var setPropertyTask = FirebaseAnalytics.SetUserPropertyAsync("test_property", "test_value");
            
            // Wait for user property setting with timeout
            float timeout = 3.0f;
            float elapsed = 0f;
            
            while (!setPropertyTask.IsCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(setPropertyTask.IsCompleted, "User property setting should complete within timeout");
            Assert.IsTrue(setPropertyTask.Result, "User property should be set successfully");
        }

        [UnityTest]
        public IEnumerator TestSetUserId()
        {
            // Ensure Firebase Analytics is initialized
            if (!FirebaseAnalytics.IsInitialized)
            {
                yield return TestFirebaseAnalyticsInitialization();
            }

            // Test setting user ID
            var setUserIdTask = FirebaseAnalytics.SetUserIdAsync("test_user_123");
            
            // Wait for user ID setting with timeout
            float timeout = 3.0f;
            float elapsed = 0f;
            
            while (!setUserIdTask.IsCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(setUserIdTask.IsCompleted, "User ID setting should complete within timeout");
            Assert.IsTrue(setUserIdTask.Result, "User ID should be set successfully");
        }        [Test]
        public void TestAnalyticsConfigurationAccess()
        {
            // Test that configuration can be accessed
            var config = Energy8.Firebase.Analytics.Configuration.FirebaseAnalyticsConfiguration.Instance;
            Assert.IsNotNull(config, "Firebase Analytics configuration should be accessible");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            FirebaseAnalytics.ResetForTesting();
        }
    }
}
