using System.ComponentModel;
using System.Runtime.Serialization;
using Domain.Common;

namespace Domain.Enums
{
    [TypeConverter(typeof(QuestTypeEnumConverter))]
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
