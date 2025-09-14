using System;
using DingoUnityExtensions.MathAndGeometry;
using Newtonsoft.Json;

namespace DingoUnityExtensions.Serialization.Converters
{
    public class Vector2DComaStringConverter : JsonConverter<Vector2D>
    {
        public override Vector2D ReadJson(JsonReader reader, Type objectType, Vector2D existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.Value as string ?? throw new JsonSerializationException("Expected string value.");
            var parts = str.Split(',');

            if (parts.Length != 2 || !double.TryParse(parts[0], out var x) || !double.TryParse(parts[1], out var y))
                throw new JsonSerializationException($"Invalid Vector2D format: {str}");

            return new Vector2D(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2D value, JsonSerializer serializer)
        {
            var str = $"{value.x},{value.y}";
            writer.WriteValue(str);
        }
    }
}