using Application.Dtos.Labels;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Helpers;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class SeasonalQuestProfile : Profile
    {
        public SeasonalQuestProfile()
        {
            CreateMap<QuestMetadata, GetSeasonalQuestDto>()
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src =>
                    src.QuestLabels.Select(ql => new GetQuestLabelDto
                    {
                        Id = ql.QuestLabel.Id,
                        Value = ql.QuestLabel.Value,
                        BackgroundColor = ql.QuestLabel.BackgroundColor,
                        TextColor = ql.QuestLabel.TextColor
                    }).ToList()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.SeasonalQuest!.Priority.ToString()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.SeasonalQuest!.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.SeasonalQuest!.Description))
                .ForMember(dest => dest.Season, opt => opt.MapFrom(src => src.SeasonalQuest!.Season.ToString()))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.SeasonalQuest!.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.SeasonalQuest!.EndDate))
                .ForMember(dest => dest.Emoji, opt => opt.MapFrom(src => src.SeasonalQuest!.Emoji))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.SeasonalQuest!.IsCompleted));

            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<SeasonalQuest, GetSeasonalQuestDto>()
                .ForMember(dest => dest.Season, opt => opt.MapFrom(src => src.Season.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Labels, opt => opt.Ignore());

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateSeasonalQuestDto, SeasonalQuest>()
                .ForMember(dest => dest.QuestMetadata, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.Seasonal,
                    AccountId = src.AccountId,
                    QuestLabels = src.Labels.Select(ql => new QuestMetadata_QuestLabel
                    {
                        QuestLabelId = ql
                    }).ToList()
                }))
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<SeasonalQuestCompletionPatchDto, SeasonalQuest>()
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom((src, dest) => src.IsCompleted ?? dest.IsCompleted))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateSeasonalQuestDto, SeasonalQuest>()
                .ForMember(dest => dest.Priority, opt => opt.ConvertUsing(new NullableEnumConverter<PriorityEnum>(), src => src.Priority));
        }
    }
}
