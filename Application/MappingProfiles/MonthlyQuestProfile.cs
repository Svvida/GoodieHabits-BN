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
            CreateMap<QuestMetadata, GetMonthlyQuestDto>()
                .ForMember(dest => dest.QuestLabels, opt => opt.MapFrom(src =>
                    src.QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.MonthlyQuest!.Priority.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.MonthlyQuest!.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.MonthlyQuest!.Description))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.MonthlyQuest!.StartDay))
                .ForMember(dest => dest.EndDay, opt => opt.MapFrom(src => src.MonthlyQuest!.EndDay))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.MonthlyQuest!.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.MonthlyQuest!.EndDate))
                .ForMember(dest => dest.Emoji, opt => opt.MapFrom(src => src.MonthlyQuest!.Emoji))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.MonthlyQuest!.IsCompleted));

            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<MonthlyQuest, GetMonthlyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.QuestLabels, opt => opt.Ignore());

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
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom((src, dest) => src.IsCompleted ?? dest.IsCompleted))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateMonthlyQuestDto, MonthlyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
