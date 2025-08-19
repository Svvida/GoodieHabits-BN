using Application.Quests.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Quests.Mappings
{
    public class RepeatableQuestStatisticsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestStatistics, RepeatableQuestStatisticsDto>();
        }
    }
}
