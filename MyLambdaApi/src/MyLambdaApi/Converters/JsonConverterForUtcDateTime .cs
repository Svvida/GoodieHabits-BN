using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyLambdaApi.Converters
{
    public class JsonConverterForUtcDateTime : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConver,
            JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString() ?? "").ToUniversalTime();
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
