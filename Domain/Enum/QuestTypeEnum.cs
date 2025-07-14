namespace Domain.Enum
{
    //[TypeConverter(typeof(QuestTypeEnumConverter))]
    public enum QuestTypeEnum
    {
        OneTime,
        Daily,
        Weekly,
        Monthly,
        Seasonal
    }
}
