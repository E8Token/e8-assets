using System;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Manages memory allocation and deallocation for data transfers between Unity and JavaScript
    /// </summary>
    public interface IMemoryManager
    {
        /// <summary>
        /// Allocates a block of memory and returns a handle to it
        /// </summary>
        /// <param name="size">Size of memory block in bytes</param>
        /// <returns>Handle to the allocated memory</returns>
        IntPtr Allocate(int size);

        /// <summary>
        /// Frees previously allocated memory
        /// </summary>
        /// <param name="ptr">Pointer to the memory to free</param>
        void Free(IntPtr ptr);

        /// <summary>
        /// Copies data from a managed buffer to an unmanaged memory location
        /// </summary>
        /// <param name="source">Source buffer</param>
        /// <param name="destination">Destination pointer</param>
        /// <param name="length">Number of bytes to copy</param>
        void CopyToUnmanaged(byte[] source, IntPtr destination, int length);

        /// <summary>
        /// Copies data from an unmanaged memory location to a managed buffer
        /// </summary>
        /// <param name="source">Source pointer</param>
        /// <param name="destination">Destination buffer</param>
        /// <param name="length">Number of bytes to copy</param>
        void CopyToManaged(IntPtr source, byte[] destination, int length);

        /// <summary>
        /// Allocates memory, writes string data to it, and returns the pointer
        /// </summary>
        /// <param name="text">String to write to memory</param>
        /// <returns>Pointer to allocated memory containing the string</returns>
        IntPtr StringToPtr(string text);

        /// <summary>
        /// Reads a string from unmanaged memory
        /// </summary>
        /// <param name="ptr">Pointer to memory containing string data</param>
        /// <returns>String read from memory</returns>
        string PtrToString(IntPtr ptr);
        
        /// <summary>
        /// Frees all allocated memory
        /// </summary>
        void ReleaseAll();
    }
}