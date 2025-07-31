using Application.QuestLabels.Queries.GetUserLabels;
using AutoMapper;
using Domain.Models;

namespace Application.Quests.Queries
{
    public class QuestLabelProfile : Profile
    {
        public QuestLabelProfile()
        {
            // Entity -> DTO
            CreateMap<QuestLabel, GetQuestLabelDto>();

            CreateMap<Quest_QuestLabel, GetQuestLabelDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QuestLabel.Id))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.QuestLabel.Value))
                .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.QuestLabel.BackgroundColor));
        }
    }
}
