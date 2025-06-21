using Application.Dtos.Labels;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestLabelProfile : Profile
    {
        public QuestLabelProfile()
        {
            CreateMap<QuestLabel, QuestLabel>();
            // Entity -> DTO
            CreateMap<QuestLabel, GetQuestLabelDto>();

            CreateMap<Quest_QuestLabel, GetQuestLabelDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QuestLabel.Id))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.QuestLabel.Value))
                .ForMember(dest => dest.BackgroundColor, opt => opt.MapFrom(src => src.QuestLabel.BackgroundColor));

            // Create DTO -> Entity
            CreateMap<CreateQuestLabelDto, QuestLabel>();

            // Patch DTO -> Entity
            CreateMap<PatchQuestLabelDto, QuestLabel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
