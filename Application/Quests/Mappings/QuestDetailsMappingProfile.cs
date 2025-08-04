using Application.Quests.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Quests.Mappings
{
    public class QuestDetailsMappingProfile : Profile
    {
        public QuestDetailsMappingProfile()
        {
            CreateMap<Quest, QuestDetailsDto>()
                .ForCtorParam(nameof(QuestDetailsDto.QuestType), opt => opt.MapFrom(src => src.QuestType.ToString()))
                .ForCtorParam(nameof(QuestDetailsDto.Priority), opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForCtorParam(nameof(QuestDetailsDto.Difficulty), opt => opt.MapFrom(src => src.Difficulty.ToString()))
                .ForCtorParam(nameof(QuestDetailsDto.Labels), opt => opt.MapFrom(src => src.Quest_QuestLabels))
                .Include<Quest, OneTimeQuestDetailsDto>()
                .Include<Quest, DailyQuestDetailsDto>()
                .Include<Quest, WeeklyQuestDetailsDto>()
                .Include<Quest, MonthlyQuestDetailsDto>()
                .Include<Quest, SeasonalQuestDetailsDto>();

            CreateMap<Quest, OneTimeQuestDetailsDto>();

            CreateMap<Quest, DailyQuestDetailsDto>()
                .ForCtorParam(nameof(DailyQuestDetailsDto.Statistics), opt => opt.MapFrom(src => src.Statistics));

            CreateMap<Quest, WeeklyQuestDetailsDto>()
                .ForCtorParam(nameof(WeeklyQuestDetailsDto.Weekdays), opt => opt.MapFrom(src =>
                    src.WeeklyQuest_Days.Select(wq => wq.Weekday.ToString())))
                .ForCtorParam(nameof(WeeklyQuestDetailsDto.Statistics), opt => opt.MapFrom(src => src.Statistics));

            CreateMap<Quest, MonthlyQuestDetailsDto>()
                .ForCtorParam(nameof(MonthlyQuestDetailsDto.StartDay), opt => opt.MapFrom(src => src.MonthlyQuest_Days!.StartDay))
                .ForCtorParam(nameof(MonthlyQuestDetailsDto.EndDay), opt => opt.MapFrom(src => src.MonthlyQuest_Days!.EndDay))
                .ForCtorParam(nameof(MonthlyQuestDetailsDto.Statistics), opt => opt.MapFrom(src => src.Statistics));

            CreateMap<Quest, SeasonalQuestDetailsDto>()
                .ForCtorParam(nameof(SeasonalQuestDetailsDto.Season), opt => opt.MapFrom(src => src.SeasonalQuest_Season!.Season.ToString()));
        }
    }
}