using System;
using Newtonsoft.Json;

namespace DingoUnityExtensions.Serialization.Converters
{
    public class UTCDateTimeConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            var ts = serializer.Deserialize<long>(reader);

            return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
        }

        public override bool CanConvert(Type type)
        {
            return typeof(DateTime).IsAssignableFrom(type);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            return;
        }

        public override bool CanRead => true;
    }
    
    public class BoolOrUtcDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DateTime).IsAssignableFrom(objectType) || objectType == typeof(bool?) || objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    return reader.Value;
                case JsonToken.Integer:
                    var ts = serializer.Deserialize<long>(reader);
                    return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case bool b:
                    writer.WriteValue(b);
                    break;
                case DateTime dt:
                    var unixTime = new DateTimeOffset(dt).ToUnixTimeSeconds();
                    writer.WriteValue(unixTime);
                    break;
                default:
                    writer.WriteNull();
                    break;
            }
        }
    }
}