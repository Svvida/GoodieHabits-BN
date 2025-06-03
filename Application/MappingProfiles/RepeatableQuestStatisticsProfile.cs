using Application.Dtos.Quests;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class RepeatableQuestStatisticsProfile : Profile
    {
        public RepeatableQuestStatisticsProfile()
        {
            // Entity -> DTO for quest statistics
            CreateMap<QuestStatistics, RepeatableQuestStatisticsDto>();
        }
    }
}
