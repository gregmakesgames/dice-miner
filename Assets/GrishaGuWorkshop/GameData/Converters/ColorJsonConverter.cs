using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GrishaGuWorkshop.GameData.Converters
{
    public sealed class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color ReadJson(
            JsonReader reader,
            Type objectType,
            Color existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Color.white;
            }

            JObject obj = JObject.Load(reader);
            return new Color(
                obj.Value<float?>("r") ?? 0f,
                obj.Value<float?>("g") ?? 0f,
                obj.Value<float?>("b") ?? 0f,
                obj.Value<float?>("a") ?? 1f);
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }
    }
}
