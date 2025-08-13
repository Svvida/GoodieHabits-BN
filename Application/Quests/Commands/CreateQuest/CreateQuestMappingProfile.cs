using Domain.Enums;
using Mapster;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateOneTimeQuestRequest, CreateOneTimeQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.OneTime);

            config.NewConfig<CreateDailyQuestRequest, CreateDailyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Daily);

            config.NewConfig<CreateWeeklyQuestRequest, CreateWeeklyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Weekly);

            config.NewConfig<CreateMonthlyQuestRequest, CreateMonthlyQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Monthly);

            config.NewConfig<CreateSeasonalQuestRequest, CreateSeasonalQuestCommand>()
                .Map(dest => dest.QuestType, src => QuestTypeEnum.Seasonal);
        }
    }
}
