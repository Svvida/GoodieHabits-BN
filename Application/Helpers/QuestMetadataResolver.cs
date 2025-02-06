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
                return context.Mapper.Map<OneTimeQuestDto>(source.OneTimeQuest);
            if (source.DailyQuest is not null)
                return context.Mapper.Map<DailyQuestDto>(source.DailyQuest);
            if (source.WeeklyQuest is not null)
                return context.Mapper.Map<WeeklyQuestDto>(source.WeeklyQuest);
            if (source.MonthlyQuest is not null)
                return context.Mapper.Map<MonthlyQuestDto>(source.MonthlyQuest);
            if (source.SeasonalQuest is not null)
                return context.Mapper.Map<SeasonalQuestDto>(source.SeasonalQuest);

            return null; // No quest type found
        }
    }
}
