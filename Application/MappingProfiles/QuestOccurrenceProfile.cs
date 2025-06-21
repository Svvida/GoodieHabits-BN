using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestOccurrenceProfile : Profile
    {
        public QuestOccurrenceProfile()
        {
            CreateMap<QuestOccurrence, QuestOccurrence>();
        }
    }
}
