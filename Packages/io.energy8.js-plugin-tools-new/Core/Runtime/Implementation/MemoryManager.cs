using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Energy8.JSPluginTools.Core.Implementation
{
    /// <summary>
    /// Implementation of the IMemoryManager interface for managing memory allocations between Unity and JavaScript
    /// </summary>
    public class MemoryManager : IMemoryManager
    {
        private readonly Dictionary<IntPtr, int> _allocations = new Dictionary<IntPtr, int>();
        private readonly object _syncRoot = new object();

        /// <inheritdoc/>
        public IntPtr Allocate(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Memory allocation size must be greater than zero", nameof(size));
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            
            lock (_syncRoot)
            {
                _allocations[ptr] = size;
            }
            
            return ptr;
        }

        /// <inheritdoc/>
        public void Free(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_allocations.ContainsKey(ptr))
                {
                    Marshal.FreeHGlobal(ptr);
                    _allocations.Remove(ptr);
                }
                else
                {
                    Debug.LogWarning($"JSPluginTools: Attempted to free unallocated memory at {ptr}");
                }
            }
        }

        /// <inheritdoc/>
        public void CopyToUnmanaged(byte[] source, IntPtr destination, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == IntPtr.Zero)
            {
                throw new ArgumentException("Destination pointer cannot be zero", nameof(destination));
            }

            if (length <= 0 || length > source.Length)
            {
                throw new ArgumentException($"Invalid length: {length}. Source length: {source.Length}", nameof(length));
            }

            Marshal.Copy(source, 0, destination, length);
        }

        /// <inheritdoc/>
        public void CopyToManaged(IntPtr source, byte[] destination, int length)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (source == IntPtr.Zero)
            {
                throw new ArgumentException("Source pointer cannot be zero", nameof(source));
            }

            if (length <= 0 || length > destination.Length)
            {
                throw new ArgumentException($"Invalid length: {length}. Destination length: {destination.Length}", nameof(length));
            }

            Marshal.Copy(source, destination, 0, length);
        }

        /// <inheritdoc/>
        public IntPtr StringToPtr(string text)
        {
            if (text == null)
            {
                return IntPtr.Zero;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            IntPtr ptr = Allocate(bytes.Length + 1);  // +1 for null terminator
            
            // Only copy bytes if there are any (empty string case)
            if (bytes.Length > 0)
            {
                CopyToUnmanaged(bytes, ptr, bytes.Length);
            }
            
            // Set null terminator
            Marshal.WriteByte(ptr + bytes.Length, 0);
            
            return ptr;
        }

        /// <inheritdoc/>
        public string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringUTF8(ptr);
        }

        /// <inheritdoc/>
        public void ReleaseAll()
        {
            lock (_syncRoot)
            {
                foreach (IntPtr ptr in new List<IntPtr>(_allocations.Keys))
                {
                    try
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSPluginTools: Error freeing memory at {ptr}: {ex.Message}");
                    }
                }
                _allocations.Clear();
            }
        }

        /// <summary>
        /// Finalizer to release unmanaged resources if not explicitly released
        /// </summary>
        ~MemoryManager()
        {
            ReleaseAll();
        }
    }
}