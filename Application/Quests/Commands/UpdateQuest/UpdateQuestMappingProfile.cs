using Domain.Enums;
using Mapster;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateOneTimeQuestRequest, UpdateOneTimeQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.OneTime);

            config.NewConfig<UpdateDailyQuestRequest, UpdateDailyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Daily);

            config.NewConfig<UpdateWeeklyQuestRequest, UpdateWeeklyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Weekly);

            config.NewConfig<UpdateMonthlyQuestRequest, UpdateMonthlyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Monthly);

            config.NewConfig<UpdateSeasonalQuestRequest, UpdateSeasonalQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Seasonal);
        }
    }
}
