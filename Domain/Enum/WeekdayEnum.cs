using System.Text.Json.Serialization;

namespace Domain.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeekdayEnum
    {
        Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
    }
}
