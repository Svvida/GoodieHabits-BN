using Application.Dtos.MonthlyQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class MonthlyQuestProfile : Profile
    {
        public MonthlyQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<MonthlyQuest, GetMonthlyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateMonthlyQuestDto, MonthlyQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.Monthly,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchMonthlyQuestDto, MonthlyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.Condition(src => src.IsCompleted.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateMonthlyQuestDto, MonthlyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
