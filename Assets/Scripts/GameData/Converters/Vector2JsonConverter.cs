using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameData.Converters
{
    public sealed class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(
            JsonReader reader,
            Type objectType,
            Vector2 existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Vector2.zero;
            }

            JObject obj = JObject.Load(reader);
            return new Vector2(
                obj.Value<float?>("x") ?? 0f,
                obj.Value<float?>("y") ?? 0f);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }
}
