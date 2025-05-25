using NUnit.Framework;
using System.Reflection;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.WebGL.PluginPlatform.Tests
{
    /// <summary>
    /// Tests for JSCallableAttribute functionality
    /// </summary>
    public class JSCallableAttributeTests
    {
        /// <summary>
        /// Test class with JSCallable methods
        /// </summary>
        public class TestClass
        {
            [JSCallable]
            public string MethodWithDefaultName()
            {
                return "default";
            }

            [JSCallable("customMethodName")]
            public string MethodWithCustomName()
            {
                return "custom";
            }

            public string MethodWithoutAttribute()
            {
                return "no attribute";
            }

            [JSCallable]
            public static string StaticMethod()
            {
                return "static";
            }

            [JSCallable("overriddenName")]
            public int MethodWithParameters(int value, string text)
            {
                return value;
            }
        }

        [Test]
        public void JSCallableAttribute_DefaultConstructor_SetsMethodNameToNull()
        {
            // Arrange & Act
            var attribute = new JSCallableAttribute();

            // Assert
            Assert.IsNull(attribute.MethodName);
        }

        [Test]
        public void JSCallableAttribute_ConstructorWithMethodName_SetsMethodNameCorrectly()
        {
            // Arrange
            var methodName = "testMethod";

            // Act
            var attribute = new JSCallableAttribute(methodName);

            // Assert
            Assert.AreEqual(methodName, attribute.MethodName);
        }

        [Test]
        public void JSCallableAttribute_CanBeAppliedToMethods()
        {
            // Arrange
            var type = typeof(TestClass);
            var method = type.GetMethod("MethodWithDefaultName");

            // Act
            var attribute = method.GetCustomAttribute<JSCallableAttribute>();

            // Assert
            Assert.IsNotNull(attribute);
            Assert.IsNull(attribute.MethodName); // Default constructor used
        }

        [Test]
        public void JSCallableAttribute_CustomMethodName_IsRetrievedCorrectly()
        {
            // Arrange
            var type = typeof(TestClass);
            var method = type.GetMethod("MethodWithCustomName");

            // Act
            var attribute = method.GetCustomAttribute<JSCallableAttribute>();

            // Assert
            Assert.IsNotNull(attribute);
            Assert.AreEqual("customMethodName", attribute.MethodName);
        }

        [Test]
        public void JSCallableAttribute_MethodWithoutAttribute_ReturnsNull()
        {
            // Arrange
            var type = typeof(TestClass);
            var method = type.GetMethod("MethodWithoutAttribute");

            // Act
            var attribute = method.GetCustomAttribute<JSCallableAttribute>();

            // Assert
            Assert.IsNull(attribute);
        }

        [Test]
        public void JSCallableAttribute_CanBeAppliedToStaticMethods()
        {
            // Arrange
            var type = typeof(TestClass);
            var method = type.GetMethod("StaticMethod");

            // Act
            var attribute = method.GetCustomAttribute<JSCallableAttribute>();

            // Assert
            Assert.IsNotNull(attribute);
        }

        [Test]
        public void JSCallableAttribute_CanBeAppliedToMethodsWithParameters()
        {
            // Arrange
            var type = typeof(TestClass);
            var method = type.GetMethod("MethodWithParameters");

            // Act
            var attribute = method.GetCustomAttribute<JSCallableAttribute>();

            // Assert
            Assert.IsNotNull(attribute);
            Assert.AreEqual("overriddenName", attribute.MethodName);
        }

        [Test]
        public void JSCallableAttribute_FindAllJSCallableMethods_ReturnsCorrectCount()
        {
            // Arrange
            var type = typeof(TestClass);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            // Act
            int jsCallableCount = 0;
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<JSCallableAttribute>() != null)
                {
                    jsCallableCount++;
                }
            }

            // Assert
            Assert.AreEqual(4, jsCallableCount); // 4 methods with JSCallable attribute
        }

        [Test]
        public void JSCallableAttribute_AttributeUsage_AllowsMultipleFalse()
        {
            // Arrange
            var attributeType = typeof(JSCallableAttribute);

            // Act
            var usage = attributeType.GetCustomAttribute<System.AttributeUsageAttribute>();

            // Assert
            Assert.IsNotNull(usage);
            Assert.AreEqual(System.AttributeTargets.Method, usage.ValidOn);
            Assert.IsFalse(usage.AllowMultiple);
        }
    }
}
