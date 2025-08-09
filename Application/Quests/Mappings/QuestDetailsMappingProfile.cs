using Application.Quests.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Quests.Mappings
{
    public class QuestDetailsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {

            config.NewConfig<Quest, OneTimeQuestDetailsDto>()
                .Map(dest => dest.QuestType, src => src.QuestType.ToString())
                .Map(dest => dest.Priority, src => src.Priority.ToString())
                .Map(dest => dest.Difficulty, src => src.Difficulty.ToString())
                .Map(dest => dest.Labels, src => src.Quest_QuestLabels);

            config.NewConfig<Quest, DailyQuestDetailsDto>()
                .Map(dest => dest.QuestType, src => src.QuestType.ToString())
                .Map(dest => dest.Priority, src => src.Priority.ToString())
                .Map(dest => dest.Difficulty, src => src.Difficulty.ToString())
                .Map(dest => dest.Labels, src => src.Quest_QuestLabels)
                .Map(dest => dest.Statistics, src => src.Statistics);

            config.NewConfig<Quest, WeeklyQuestDetailsDto>()
                .Map(dest => dest.QuestType, src => src.QuestType.ToString())
                .Map(dest => dest.Priority, src => src.Priority.ToString())
                .Map(dest => dest.Difficulty, src => src.Difficulty.ToString())
                .Map(dest => dest.Labels, src => src.Quest_QuestLabels)
                .Map(dest => dest.Weekdays, src => src.WeeklyQuest_Days.Select(wq => wq.Weekday.ToString()).ToHashSet())
                .Map(dest => dest.Statistics, src => src.Statistics);

            config.NewConfig<Quest, MonthlyQuestDetailsDto>()
                .Map(dest => dest.QuestType, src => src.QuestType.ToString())
                .Map(dest => dest.Priority, src => src.Priority.ToString())
                .Map(dest => dest.Difficulty, src => src.Difficulty.ToString())
                .Map(dest => dest.Labels, src => src.Quest_QuestLabels)
                .Map(dest => dest.StartDay, src => src.MonthlyQuest_Days!.StartDay)
                .Map(dest => dest.EndDay, src => src.MonthlyQuest_Days!.EndDay)
                .Map(dest => dest.Statistics, src => src.Statistics);

            config.NewConfig<Quest, SeasonalQuestDetailsDto>()
                .Map(dest => dest.QuestType, src => src.QuestType.ToString())
                .Map(dest => dest.Priority, src => src.Priority.ToString())
                .Map(dest => dest.Difficulty, src => src.Difficulty.ToString())
                .Map(dest => dest.Labels, src => src.Quest_QuestLabels)
                .Map(dest => dest.Season, src => src.SeasonalQuest_Season!.Season.ToString());
        }
    }
}