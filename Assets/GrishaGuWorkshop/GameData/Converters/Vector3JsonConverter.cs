using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GrishaGuWorkshop.GameData.Converters
{
    public sealed class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(
            JsonReader reader,
            Type objectType,
            Vector3 existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Vector3.zero;
            }

            JObject obj = JObject.Load(reader);
            return new Vector3(
                obj.Value<float?>("x") ?? 0f,
                obj.Value<float?>("y") ?? 0f,
                obj.Value<float?>("z") ?? 0f);
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }
    }
}
