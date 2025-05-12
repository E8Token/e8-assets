using System;
using System.Text;
using NUnit.Framework;
using Energy8.JSPluginTools.Core.Implementation;

namespace Energy8.JSPluginTools.Core.Tests.EditMode
{
    public class MemoryManagerTests
    {
        private MemoryManager _memoryManager;

        [SetUp]
        public void Setup()
        {
            _memoryManager = new MemoryManager();
        }

        [TearDown]
        public void TearDown()
        {
            _memoryManager.ReleaseAll();
            _memoryManager = null;
        }

        [Test]
        public void Allocate_ValidSize_ReturnsNonZeroPointer()
        {
            // Act
            IntPtr result = _memoryManager.Allocate(128);

            // Assert
            Assert.AreNotEqual(IntPtr.Zero, result);

            // Cleanup
            _memoryManager.Free(result);
        }

        [Test]
        public void Allocate_InvalidSize_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _memoryManager.Allocate(0));
            Assert.Throws<ArgumentException>(() => _memoryManager.Allocate(-10));
        }

        [Test]
        public void Free_ValidPointer_FreesMemory()
        {
            // Arrange
            IntPtr ptr = _memoryManager.Allocate(128);

            // Act - No exception should be thrown
            Assert.DoesNotThrow(() => _memoryManager.Free(ptr));
        }

        [Test]
        public void Free_NullPointer_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _memoryManager.Free(IntPtr.Zero));
        }

        [Test]
        public void CopyToUnmanaged_ValidParams_CopiesDataCorrectly()
        {
            // Arrange
            byte[] source = { 1, 2, 3, 4, 5 };
            int length = source.Length;
            IntPtr destination = _memoryManager.Allocate(length);

            // Act
            _memoryManager.CopyToUnmanaged(source, destination, length);

            // Assert
            byte[] result = new byte[length];
            _memoryManager.CopyToManaged(destination, result, length);
            
            CollectionAssert.AreEqual(source, result);

            // Cleanup
            _memoryManager.Free(destination);
        }

        [Test]
        public void CopyToUnmanaged_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IntPtr destination = _memoryManager.Allocate(10);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _memoryManager.CopyToUnmanaged(null, destination, 1));

            // Cleanup
            _memoryManager.Free(destination);
        }

        [Test]
        public void CopyToUnmanaged_ZeroDestination_ThrowsArgumentException()
        {
            // Arrange
            byte[] source = { 1, 2, 3 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToUnmanaged(source, IntPtr.Zero, 3));
        }

        [Test]
        public void CopyToUnmanaged_InvalidLength_ThrowsArgumentException()
        {
            // Arrange
            byte[] source = { 1, 2, 3 };
            IntPtr destination = _memoryManager.Allocate(10);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToUnmanaged(source, destination, 0));
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToUnmanaged(source, destination, 10));

            // Cleanup
            _memoryManager.Free(destination);
        }

        [Test]
        public void CopyToManaged_ValidParams_CopiesDataCorrectly()
        {
            // Arrange
            byte[] expected = { 10, 20, 30, 40, 50 };
            int length = expected.Length;
            IntPtr source = _memoryManager.Allocate(length);
            
            // Prepare source data
            for (int i = 0; i < length; i++)
            {
                System.Runtime.InteropServices.Marshal.WriteByte(source + i, expected[i]);
            }
            
            byte[] destination = new byte[length];

            // Act
            _memoryManager.CopyToManaged(source, destination, length);

            // Assert
            CollectionAssert.AreEqual(expected, destination);

            // Cleanup
            _memoryManager.Free(source);
        }

        [Test]
        public void CopyToManaged_NullDestination_ThrowsArgumentNullException()
        {
            // Arrange
            IntPtr source = _memoryManager.Allocate(10);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _memoryManager.CopyToManaged(source, null, 1));

            // Cleanup
            _memoryManager.Free(source);
        }

        [Test]
        public void CopyToManaged_ZeroSource_ThrowsArgumentException()
        {
            // Arrange
            byte[] destination = new byte[3];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToManaged(IntPtr.Zero, destination, 3));
        }

        [Test]
        public void CopyToManaged_InvalidLength_ThrowsArgumentException()
        {
            // Arrange
            byte[] destination = new byte[3];
            IntPtr source = _memoryManager.Allocate(10);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToManaged(source, destination, 0));
            Assert.Throws<ArgumentException>(() => _memoryManager.CopyToManaged(source, destination, 10));

            // Cleanup
            _memoryManager.Free(source);
        }

        [Test]
        public void StringToPtr_ValidString_ConvertsCorrectly()
        {
            // Arrange
            string text = "Hello, world!";

            // Act
            IntPtr ptr = _memoryManager.StringToPtr(text);

            // Assert
            string result = _memoryManager.PtrToString(ptr);
            Assert.AreEqual(text, result);

            // Cleanup
            _memoryManager.Free(ptr);
        }

        [Test]
        public void StringToPtr_NullString_ReturnsZeroPointer()
        {
            // Act
            IntPtr ptr = _memoryManager.StringToPtr(null);

            // Assert
            Assert.AreEqual(IntPtr.Zero, ptr);
        }

        [Test]
        public void StringToPtr_EmptyString_ConvertsCorrectly()
        {
            // Arrange
            string text = string.Empty;

            // Act
            IntPtr ptr = _memoryManager.StringToPtr(text);

            // Assert
            string result = _memoryManager.PtrToString(ptr);
            Assert.AreEqual(text, result);

            // Cleanup
            _memoryManager.Free(ptr);
        }

        [Test]
        public void PtrToString_NullPointer_ReturnsNull()
        {
            // Act
            string result = _memoryManager.PtrToString(IntPtr.Zero);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ReleaseAll_AfterAllocations_FreesAllMemory()
        {
            // Arrange - Allocate multiple memory blocks
            IntPtr ptr1 = _memoryManager.Allocate(10);
            IntPtr ptr2 = _memoryManager.Allocate(20);
            IntPtr ptr3 = _memoryManager.Allocate(30);

            // Act
            _memoryManager.ReleaseAll();

            // Assert - No direct way to verify memory was released
            // The test passes if no exceptions are thrown
            // This test mainly ensures the method executes without errors
            Assert.Pass("ReleaseAll executed without exceptions");
        }
    }
}