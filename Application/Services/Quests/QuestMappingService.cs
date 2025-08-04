using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Models;

namespace Application.Services.Quests
{
    public class QuestMappingService(IMapper mapper) : IQuestMappingService
    {
        public QuestDetailsDto MapToDto(Quest quest)
        {
            return quest.QuestType switch
            {
                QuestTypeEnum.OneTime => mapper.Map<OneTimeQuestDetailsDto>(quest),
                QuestTypeEnum.Daily => mapper.Map<DailyQuestDetailsDto>(quest),
                QuestTypeEnum.Weekly => mapper.Map<WeeklyQuestDetailsDto>(quest),
                QuestTypeEnum.Monthly => mapper.Map<MonthlyQuestDetailsDto>(quest),
                QuestTypeEnum.Seasonal => mapper.Map<SeasonalQuestDetailsDto>(quest),
                _ => throw new InvalidArgumentException("Invalid quest type")
            };
        }
    }
}
