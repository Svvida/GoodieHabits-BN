using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestStatisticsProfile : Profile
    {
        public QuestStatisticsProfile()
        {
            CreateMap<QuestStatistics, QuestStatistics>();
        }
    }
}
