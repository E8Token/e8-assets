using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Energy8.Identity.Shared.Core.Contracts.Dto.Common
{
    /// <summary>
    /// Base class for all DTOs providing common serialization and conversion functionality.
    /// </summary>
    /// <remarks>
    /// This base class implements core functionality used across all DTOs:
    /// - JSON serialization and deserialization using Unity's JsonUtility
    /// - Object to dictionary conversion
    /// - String representation
    /// 
    /// Inherit from this class to ensure consistent behavior across all DTOs in the system.
    /// </remarks>
    [System.Serializable]
    public class DtoBase
    {
        /// <summary>
        /// Returns a JSON string representation of the DTO.
        /// </summary>
        /// <returns>A JSON-formatted string representation of the current instance.</returns>
        /// <remarks>
        /// This override ensures consistent string representation across all DTOs.
        /// It internally calls <see cref="ToJson"/> for the actual serialization.
        /// </remarks>
        public override string ToString() => ToJson();

        /// <summary>
        /// Serializes the DTO to its JSON representation using Unity's JsonUtility.
        /// </summary>
        /// <returns>A JSON string containing all serializable properties of the DTO.</returns>
        /// <remarks>
        /// This method uses Unity's JsonUtility for serialization which is optimized for Unity.
        /// </remarks>
        public virtual string ToJson() => JsonUtility.ToJson(this, true);

        /// <summary>
        /// Converts the DTO to a flattened dictionary representation.
        /// </summary>
        /// <returns>A dictionary where keys are property paths and values are the corresponding property values.</returns>
        /// <remarks>
        /// The flattening process handles nested objects by creating dot-notation keys.
        /// For example, a property path might look like "Address.Street" for nested objects.
        /// Null values are excluded from the resulting dictionary.
        /// </remarks>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();

            void FlattenObject(string prefix, object obj)
            {
                if (obj == null)
                    return;

                var type = obj.GetType();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    var propertyName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    var value = property.GetValue(obj);

                    if (value == null)
                    {
                        continue;
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        FlattenObject(propertyName, value);
                    }
                    else
                    {
                        result[propertyName] = value;
                    }
                }
            }

            FlattenObject(string.Empty, this);
            return result;
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a specific DTO type using Unity's JsonUtility.
        /// </summary>
        /// <typeparam name="T">The type of DTO to create, must inherit from DtoBase.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="data">When successful, contains the deserialized DTO; otherwise, null.</param>
        /// <returns>True if deserialization was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method provides a safe way to deserialize JSON into DTOs using Unity's JsonUtility:
        /// - Returns false instead of throwing exceptions on failure
        /// - Supports null checking
        /// - Optimized for Unity environment
        /// </remarks>
        public static bool TryFromJson<T>(string json, out T data) where T : DtoBase
        {
            try
            {
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }
    }
}
