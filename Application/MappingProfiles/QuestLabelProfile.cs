using Application.Dtos.Labels;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class QuestLabelProfile : Profile
    {
        public QuestLabelProfile()
        {
            // Entity -> DTO
            CreateMap<QuestLabel, GetQuestLabelDto>();

            // Create DTO -> Entity
            CreateMap<CreateQuestLabelDto, QuestLabel>();

            // Patch DTO -> Entity
            CreateMap<PatchQuestLabelDto, QuestLabel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
