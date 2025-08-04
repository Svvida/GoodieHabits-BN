using Application.QuestLabels.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.QuestLabels.Mappings
{
    public class QuestLabelMappingProfile : Profile
    {
        public QuestLabelMappingProfile()
        {
            CreateMap<QuestLabel, QuestLabelDto>();

            CreateMap<Quest_QuestLabel, QuestLabelDto>()
                .ForCtorParam(nameof(QuestLabelDto.Id), opt => opt.MapFrom(src => src.QuestLabel.Id))
                .ForCtorParam(nameof(QuestLabelDto.Value), opt => opt.MapFrom(src => src.QuestLabel.Value))
                .ForCtorParam(nameof(QuestLabelDto.BackgroundColor), opt => opt.MapFrom(src => src.QuestLabel.BackgroundColor));
        }
    }
}
