using System;
using Newtonsoft.Json;

namespace DingoUnityExtensions.Serialization.Converters
{
    public static class UTDDatesConvert
    {
        public static DateTime LongUnixSecondsToUTC(long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }

        public static DateTime LongUnixSecondsToLocalUTC(long seconds)
        {
            var utcDateTime = LongUnixSecondsToUTC(seconds);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
        }
        
        public static DateTime LongUnixSecondsToUTC(long seconds, TimeZoneInfo timeZoneInfo)
        {
            var utcDateTime = LongUnixSecondsToUTC(seconds);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
        }
    }
    
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

            var utcDateTime = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
            return utcDateTime;
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
            if (value is DateTime dt)
            {
                var unixTime = new DateTimeOffset(dt).ToUnixTimeSeconds();
                writer.WriteValue(unixTime);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override bool CanRead => true;
    }
    
    public class ToLocalUTCDateTimeConverter : JsonConverter
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
            return UTDDatesConvert.LongUnixSecondsToLocalUTC(ts);
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
            if (value is DateTime dt)
            {
                dt = TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.Local);
                var unixTime = new DateTimeOffset(dt).ToUnixTimeSeconds();
                writer.WriteValue(unixTime);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override bool CanRead => true;
    }
}