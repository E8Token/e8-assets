using System;
using System.Collections.Generic;

using UnityEngine;
using Newtonsoft.Json;

using Energy8.Identity.Shared.Core.Contracts.Dto.Common;
using Energy8.Identity.Http.Core;

namespace Energy8.Identity.Http.Runtime.Serializers
{
    /// <summary>
    /// Сериализатор для формат WWWForm (application/x-www-form-urlencoded)
    /// Используется по умолчанию в Unity HTTP клиенте
    /// </summary>
    public class WWWFormSerializer : IRequestSerializer
    {
        public byte[] Serialize(object data)
        {
            if (data == null)
            {
                return null;
            }

            var formData = new WWWForm();

            if (data is DtoBase model)
            {
                // Сериализация DtoBase
                var dictionary = model.ToDictionary();
                foreach (var pair in dictionary)
                {
                    if (pair.Value != null)
                    {
                        formData.AddField(pair.Key, pair.Value.ToString());
                    }
                }
            }
            else
            {
                // Сериализация любого объекта через JSON и преобразование в словарь
                var dictionary = data as IDictionary<string, object>;
                
                if (dictionary == null)
                {
                    // Пробуем десериализовать JSON и получить словарь
                    try
                    {
                        var json = JsonConvert.SerializeObject(data);
                        dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to serialize data to WWWForm: {ex.Message}", ex);
                    }
                }

                if (dictionary != null)
                {
                    foreach (var pair in dictionary)
                    {
                        if (pair.Value != null)
                        {
                            formData.AddField(pair.Key, pair.Value.ToString());
                        }
                    }
                }
            }

            return formData.data;
        }

        public Dictionary<string, string> GetHeaders()
        {
            // WWWForm автоматически устанавливает правильный Content-Type
            var formData = new WWWForm();
            return formData.headers;
        }
    }
}
