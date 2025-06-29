using UnityEngine;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Utility class for input system compatibility
    /// Provides safe methods to check input regardless of which input system is active
    /// </summary>
    public static class InputSystemCompatibility
    {
        /// <summary>
        /// Safely checks if a key is pressed, compatible with both Legacy Input and Input System
        /// </summary>
        /// <param name="keyCode">The key to check</param>
        /// <returns>True if key was pressed this frame, false otherwise</returns>
        public static bool GetKeyDown(KeyCode keyCode)
        {
            try
            {
                // Try using legacy input system
                return Input.GetKeyDown(keyCode);
            }
            catch (System.InvalidOperationException)
            {
                // Legacy input is disabled and Input System is active
                // For now, return false. In future, implement Input System support here
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Input system compatibility error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Safely checks if a key is currently held down
        /// </summary>
        /// <param name="keyCode">The key to check</param>
        /// <returns>True if key is currently pressed, false otherwise</returns>
        public static bool GetKey(KeyCode keyCode)
        {
            try
            {
                return Input.GetKey(keyCode);
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Input system compatibility error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Safely checks if a key was released this frame
        /// </summary>
        /// <param name="keyCode">The key to check</param>
        /// <returns>True if key was released this frame, false otherwise</returns>
        public static bool GetKeyUp(KeyCode keyCode)
        {
            try
            {
                return Input.GetKeyUp(keyCode);
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Input system compatibility error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the new Input System is active
        /// </summary>
        /// <returns>True if Input System is active, false if Legacy Input is active</returns>
        public static bool IsInputSystemActive()
        {
            try
            {
                Input.GetKey(KeyCode.Space);
                return false; // Legacy input is working
            }
            catch (System.InvalidOperationException)
            {
                return true; // Input System is active
            }
            catch
            {
                return false; // Unknown state, assume legacy
            }
        }

        /// <summary>
        /// Gets information about the current input system
        /// </summary>
        /// <returns>String describing the active input system</returns>
        public static string GetInputSystemInfo()
        {
            if (IsInputSystemActive())
            {
                return "Input System Package (Active)";
            }
            else
            {
                return "Legacy Input Manager (Active)";
            }
        }

        // Future: Add Input System specific methods when the package is available
        /*
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static bool GetKeyDownInputSystem(KeyCode keyCode)
        {
            // Implement Input System key checking
            // Example: return Keyboard.current[ConvertKeyCode(keyCode)].wasPressedThisFrame;
            return false;
        }

        private static Key ConvertKeyCode(KeyCode keyCode)
        {
            // Convert Unity KeyCode to Input System Key
            // This would need a comprehensive mapping
            return Key.Space;
        }
        #endif
        */
    }
}
