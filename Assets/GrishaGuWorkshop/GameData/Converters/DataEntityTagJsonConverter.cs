using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GrishaGuWorkshop.Converters
{
    public sealed class DataEntityTagJsonConverter : JsonConverter<DataEntityTag>
    {
        [ThreadStatic]
        private static bool isReading;

        [ThreadStatic]
        private static bool isWriting;

        public override bool CanRead => !isReading;
        public override bool CanWrite => !isWriting;

        public override DataEntityTag ReadJson(
            JsonReader reader,
            Type objectType,
            DataEntityTag existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject wrapper = JObject.Load(reader);
            if (wrapper.Count != 1)
            {
                Debug.LogWarning(
                    $"DataEntityTag must be a single-property object with the type full name as key. Got {wrapper.Count} properties.");
                return null;
            }

            JProperty prop = wrapper.Properties().First();
            Type resolvedType = ResolveTagType(prop.Name);
            if (resolvedType == null)
            {
                Debug.LogWarning($"DataEntityTag type '{prop.Name}' could not be resolved. Skipped.");
                return null;
            }

            isReading = true;
            try
            {
                return (DataEntityTag)prop.Value.ToObject(resolvedType, serializer);
            }
            finally
            {
                isReading = false;
            }
        }

        public override void WriteJson(JsonWriter writer, DataEntityTag value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();
            writer.WriteStartObject();
            writer.WritePropertyName(type.FullName);
            isWriting = true;
            try
            {
                serializer.Serialize(writer, value, type);
            }
            finally
            {
                isWriting = false;
            }

            writer.WriteEndObject();
        }

        private static Type ResolveTagType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type;
                try
                {
                    type = assembly.GetType(fullName);
                }
                catch
                {
                    continue;
                }

                if (type != null && typeof(DataEntityTag).IsAssignableFrom(type))
                {
                    return type;
                }
            }

            return null;
        }
    }
}
