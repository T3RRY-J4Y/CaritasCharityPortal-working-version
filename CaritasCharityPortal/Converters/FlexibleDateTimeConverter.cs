using Newtonsoft.Json;
using System;

namespace CaritasCharityPortal.Converters
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Date:
                    return (DateTime)reader.Value;
                case JsonToken.Integer:
                    long ms = (long)reader.Value;
                    return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                case JsonToken.String:
                    string s = reader.Value.ToString();
                    if (long.TryParse(s, out long longMs))
                        return DateTimeOffset.FromUnixTimeMilliseconds(longMs).UtcDateTime;
                    return DateTime.Parse(s);
                default:
                    throw new JsonSerializationException($"Unexpected token type for DateTime: {reader.TokenType}");
            }
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}