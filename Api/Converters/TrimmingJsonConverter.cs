using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Converters
{
    public class TrimmingJsonConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Apply trimming only to strings
            return typeToConvert == typeof(string);
        }

        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Read the string value and trim it if it is not null
            return reader.GetString()?.Trim();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            // Write the string value as is, without trimming
            writer.WriteStringValue(value);
        }
    }
}
