using Application.Dtos.Labels;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class WeeklyQuestProfile : Profile
    {
        public WeeklyQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<Quest, GetWeeklyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestType))
                .ForMember(dest => dest.Weekdays, opt => opt.MapFrom(src => src.WeeklyQuest_Days.Select(wq =>
                    wq.Weekday.ToString()).ToList()))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Quest_QuestLabels.Select(ql => new GetQuestLabelDto
                {
                    Id = ql.QuestLabelId,
                    Value = ql.QuestLabel.Value,
                    BackgroundColor = ql.QuestLabel.BackgroundColor,
                    TextColor = ql.QuestLabel.TextColor
                }).ToList()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateWeeklyQuestDto, Quest>()
                .ForMember(dest => dest.Quest_QuestLabels, opt => opt.MapFrom(src => src.Labels.Select(ql => new Quest_QuestLabel
                {
                    QuestLabelId = ql
                }).ToList()))
                .ForMember(dest => dest.WeeklyQuest_Days, opt => opt.MapFrom(src => src.Weekdays.Select(wd => new WeeklyQuest_Day
                {
                    Weekday = Enum.Parse<WeekdayEnum>(wd)
                }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<WeeklyQuestCompletionPatchDto, Quest>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateWeeklyQuestDto, Quest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
