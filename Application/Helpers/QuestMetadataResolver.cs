using Application.Dtos.DailyQuest;
using Application.Dtos.MonthlyQuest;
using Application.Dtos.OneTimeQuest;
using Application.Dtos.QuestMetadata;
using Application.Dtos.SeasonalQuest;
using Application.Dtos.WeeklyQuest;
using AutoMapper;
using Domain.Models;

namespace Application.Helpers
{
    public class QuestMetadataResolver : IValueResolver<QuestMetadata, QuestMetadataDto, object?>
    {
        public QuestMetadataResolver() { }
        public object? Resolve(QuestMetadata source, QuestMetadataDto destination, object? destMember, ResolutionContext context)
        {
            // Return only the non-null quest type
            if (source.OneTimeQuest is not null)
                return context.Mapper.Map<GetOneTimeQuestDto>(source.OneTimeQuest);
            if (source.DailyQuest is not null)
                return context.Mapper.Map<GetDailyQuestDto>(source.DailyQuest);
            if (source.WeeklyQuest is not null)
                return context.Mapper.Map<GetWeeklyQuestDto>(source.WeeklyQuest);
            if (source.MonthlyQuest is not null)
                return context.Mapper.Map<GetMonthlyQuestDto>(source.MonthlyQuest);
            if (source.SeasonalQuest is not null)
                return context.Mapper.Map<GetSeasonalQuestDto>(source.SeasonalQuest);

            return null; // No quest type found
        }
    }
}
