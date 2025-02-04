using Application.Dtos.SeasonalQuest;
using AutoMapper;
using Domain.Enum;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class SeasonalQuestProfile : Profile
    {
        public SeasonalQuestProfile()
        {
            // Entity -> DTO (Convert Enum -> String for Response)
            CreateMap<SeasonalQuest, SeasonalQuestDto>()
                .ForMember(dest => dest.Season, opt => opt.MapFrom(src => src.Season.ToString()));

            // Create DTO -> Entity (Convert String -> Enum)
            CreateMap<CreateSeasonalQuestDto, SeasonalQuest>()
                .ForMember(dest => dest.Quest, opt => opt.MapFrom(src => new QuestMetadata
                {
                    QuestType = QuestTypeEnum.Seasonal,
                    AccountId = src.AccountId
                }));

            // Patch DTO -> Entity (Convert String -> Enum, Ignore Nulls)
            CreateMap<PatchSeasonalQuestDto, SeasonalQuest>()
                .ForMember(dest => dest.IsCompleted, opt => opt.Condition(src => src.IsCompleted.HasValue))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update DTO -> Entity (Convert String -> Enum)
            CreateMap<UpdateSeasonalQuestDto, SeasonalQuest>();
        }
    }
}
