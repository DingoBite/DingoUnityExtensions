using System;
using System.Linq;
using Newtonsoft.Json;

namespace DingoUnityExtensions.Serialization.Converters
{
    public class FlagConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader,  Type objectType, object existingValue, JsonSerializer serializer)
        {
            var outVal = 0;
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Read();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    outVal += (int)Enum.Parse(objectType, reader.Value.ToString());
                    reader.Read();
                }
            }
            return outVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var flags = value.ToString()
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => $"\"{f}\"");

                writer.WriteRawValue($"[{string.Join(", ", flags)}]");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}