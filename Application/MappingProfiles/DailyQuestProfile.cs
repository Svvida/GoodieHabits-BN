using Application.Dtos.Quests.DailyQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class DailyQuestProfile : Profile
    {
        public DailyQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<DailyQuest, GetDailyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateDailyQuestDto, DailyQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.Daily,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchDailyQuestDto, DailyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.Condition(src => src.IsCompleted.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateDailyQuestDto, DailyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
