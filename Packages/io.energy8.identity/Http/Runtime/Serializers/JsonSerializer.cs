using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Energy8.Identity.Http.Core;

namespace Energy8.Identity.Http.Runtime.Serializers
{
    /// <summary>
    /// Сериализатор для формат JSON (application/json)
    /// Используется для REST API с JSON payloads
    /// </summary>
    public class JsonSerializer : IRequestSerializer
    {
        private readonly JsonSerializerSettings settings;

        /// <summary>
        /// Создаёт сериализатор с настройками по умолчанию
        /// </summary>
        public JsonSerializer()
        {
            settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

        /// <summary>
        /// Создаёт сериализатор с кастомными настройками
        /// </summary>
        /// <param name="customSettings">Кастомные настройки JSON сериализации</param>
        public JsonSerializer(JsonSerializerSettings customSettings)
        {
            settings = customSettings ?? throw new ArgumentNullException(nameof(customSettings));
        }

        public byte[] Serialize(object data)
        {
            if (data == null)
            {
                return null;
            }

            try
            {
                var json = JsonConvert.SerializeObject(data, settings);
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to serialize data to JSON: {ex.Message}", ex);
            }
        }

        public Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                { "Content-Type", "application/json; charset=utf-8" },
                { "Accept", "application/json" }
            };
        }
    }
}
