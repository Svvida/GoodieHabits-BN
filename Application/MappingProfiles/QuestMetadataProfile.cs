using Application.Dtos.QuestMetadata;
using AutoMapper;
using Domain.Common;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestMetadataProfile : Profile
    {
        public QuestMetadataProfile()
        {
            // Map QuestMetadata -> GetQuestMetadataDto
            CreateMap<QuestMetadata, GetQuestMetadataDto>()
                .IncludeMembers(src => src.GetActualQuest());

            // Explicitly map QuestBase -> GetQuestMetadataDto
            CreateMap<QuestBase, GetQuestMetadataDto>();

            // Map each subclass of QuestBase directly to GetQuestMetadataDto
            CreateMap<OneTimeQuest, GetQuestMetadataDto>();
            CreateMap<DailyQuest, GetQuestMetadataDto>();
            CreateMap<WeeklyQuest, GetQuestMetadataDto>();
            CreateMap<MonthlyQuest, GetQuestMetadataDto>();
            CreateMap<SeasonalQuest, GetQuestMetadataDto>();
        }
    }
}
