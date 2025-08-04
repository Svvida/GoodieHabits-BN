using Application.Quests.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Quests.Mappings
{
    public class RepeatableQuestStatisticsMappingProfile : Profile
    {
        public RepeatableQuestStatisticsMappingProfile()
        {
            CreateMap<QuestStatistics, RepeatableQuestStatisticsDto>();
        }
    }
}
