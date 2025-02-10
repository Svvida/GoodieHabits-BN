using Application.Dtos.OneTimeQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class OneTimeQuestProfile : Profile
    {
        public OneTimeQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<OneTimeQuest, GetOneTimeQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.OneTime,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.Condition(src => src.IsCompleted.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateOneTimeQuestDto, OneTimeQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
