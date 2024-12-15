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
            var ts = serializer.Deserialize<long>(reader);

            return DateTime.FromFileTimeUtc(ts);
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
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
    }
}