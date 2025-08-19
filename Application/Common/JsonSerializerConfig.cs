using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Common
{
    public static class JsonSerializerConfig
    {
        public static readonly JsonSerializerOptions CaseInsensitveOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
