using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using MapsterMapper;

namespace Application.Quests
{
    public class QuestMapper(IMapper mapper) : IQuestMapper
    {
        private static readonly Dictionary<QuestTypeEnum, Type> QuestTypeMappings = new()
        {
            { QuestTypeEnum.OneTime, typeof(OneTimeQuestDetailsDto) },
            { QuestTypeEnum.Daily, typeof(DailyQuestDetailsDto) },
            { QuestTypeEnum.Weekly, typeof(WeeklyQuestDetailsDto) },
            { QuestTypeEnum.Monthly, typeof(MonthlyQuestDetailsDto) },
            { QuestTypeEnum.Seasonal, typeof(SeasonalQuestDetailsDto) }
        };

        public QuestDetailsDto MapToDto(Quest quest)
        {
            if (!QuestTypeMappings.TryGetValue(quest.QuestType, out var targetType))
                throw new InvalidArgumentException($"Invalid quest type: {quest.QuestType}");

            return (QuestDetailsDto)mapper.Map(quest, quest.GetType(), targetType);
        }
    }
}
