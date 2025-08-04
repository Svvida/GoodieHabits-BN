using System.Runtime.Serialization;

namespace Domain.Enum
{
    //[TypeConverter(typeof(QuestTypeEnumConverter))]
    public enum QuestTypeEnum
    {
        [EnumMember(Value = "one-time")]
        OneTime,
        Daily,
        Weekly,
        Monthly,
        Seasonal
    }
}
