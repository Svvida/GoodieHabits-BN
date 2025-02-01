using Application.Dtos.RepeatableQuest;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class RepeatableQuestProfile : Profile
    {
        public RepeatableQuestProfile()
        {
            // Entity -> DTO
            CreateMap<RepeatableQuest, RepeatableQuestDto>();

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateRepeatableQuestDto, RepeatableQuest>()
                .ForMember(dest => dest.Quest, opt => opt.MapFrom(src => new Quest
                {
                    QuestType = QuestType.Repeatable,
                    AccountId = src.AccountId
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<Priority>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchRepeatableQuestDto, RepeatableQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<Priority>(), src => src.Priority))
                .ForMember(dest => dest.IsCompleted, opt => opt.Condition(src => src.IsCompleted.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateRepeatableQuestDto, RepeatableQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<Priority>(), src => src.Priority));

            // Polymorphic Mapping for RepeatInterval -> RepeatIntervalDto
            CreateMap<RepeatInterval, RepeatIntervalDto>()
                .Include<DailyRepeatInterval, DailyRepeatIntervalDto>()
                .Include<WeeklyRepeatInterval, WeeklyRepeatIntervalDto>()
                .Include<MonthlyRepeatInterval, MonthlyRepeatIntervalDto>();

            // Polymorphic Mapping for RepeatIntervalDto -> RepeatInterval
            CreateMap<RepeatIntervalDto, RepeatInterval>()
                .Include<DailyRepeatIntervalDto, DailyRepeatInterval>()
                .Include<WeeklyRepeatIntervalDto, WeeklyRepeatInterval>()
                .Include<MonthlyRepeatIntervalDto, MonthlyRepeatInterval>();

            // Polymorphic Mapping for RepeatInterval subtypes
            CreateMap<DailyRepeatIntervalDto, DailyRepeatInterval>()
                .ReverseMap();
            CreateMap<WeeklyRepeatIntervalDto, WeeklyRepeatInterval>()
                .ReverseMap();
            CreateMap<MonthlyRepeatIntervalDto, MonthlyRepeatInterval>()
                .ReverseMap();
        }
    }
}
