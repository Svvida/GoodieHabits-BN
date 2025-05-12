using Application.Dtos.Labels;
using Application.Dtos.Quests.MonthlyQuest;
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
            CreateMap<Quest, GetMonthlyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestType))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.Quest_QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                    }).ToList()))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.MonthlyQuest_Days!.StartDay))
                .ForMember(dest => dest.EndDay, opt => opt.MapFrom(src => src.MonthlyQuest_Days!.EndDay));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateMonthlyQuestDto, Quest>()
                .ForMember(dest => dest.Quest_QuestLabels, opt => opt.MapFrom(src => src.Labels.Select(ql => new Quest_QuestLabel
                {
                    QuestLabelId = ql
                }).ToList()))
                .ForMember(dest => dest.MonthlyQuest_Days, opt => opt.MapFrom(src => new MonthlyQuest_Days
                {
                    StartDay = src.StartDay,
                    EndDay = src.EndDay
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<MonthlyQuestCompletionPatchDto, Quest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateMonthlyQuestDto, Quest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority))
                .ForPath(dest => dest.MonthlyQuest_Days!.StartDay, opt => opt.MapFrom(src => src.StartDay))
                .ForPath(dest => dest.MonthlyQuest_Days!.EndDay, opt => opt.MapFrom(src => src.EndDay));
        }
    }
}
