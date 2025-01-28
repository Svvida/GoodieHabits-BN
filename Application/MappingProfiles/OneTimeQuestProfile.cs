using Application.Dtos.OneTimeQuest;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class OneTimeQuestProfile : Profile
    {
        public OneTimeQuestProfile()
        {
            // Entity -> DTO
            CreateMap<OneTimeQuest, OneTimeQuestDto>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Quest.AccountId));

            // Create DTO -> Entity
            CreateMap<CreateOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.Quest, opt => opt.MapFrom(src => new Quest
                {
                    QuestType = QuestType.OneTime,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority));

            // Patch Dto -> Entity
            CreateMap<PatchOneTimeQuestDto, OneTimeQuest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity
            CreateMap<UpdateOneTimeQuestDto, OneTimeQuest>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
