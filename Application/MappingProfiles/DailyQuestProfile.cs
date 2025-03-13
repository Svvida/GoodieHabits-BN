using Application.Dtos.Labels;
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
            CreateMap<QuestMetadata, GetDailyQuestDto>()
                .ForMember(dest => dest.QuestLabels, opt => opt.MapFrom(src =>
                    src.QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.DailyQuest!.Priority.ToString()));

            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<DailyQuest, GetDailyQuestDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.QuestLabels, opt => opt.Ignore());

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
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom((src, dest) => src.IsCompleted ?? dest.IsCompleted))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateDailyQuestDto, DailyQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
