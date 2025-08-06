using Mapster;

namespace Application.Quests.CreateQuest
{
    public class CreateQuestMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateOneTimeQuestRequest, CreateOneTimeQuestCommand>()
                .Map(dest => dest.QuestType, src => Domain.Enum.QuestTypeEnum.OneTime);

            config.NewConfig<CreateDailyQuestRequest, CreateDailyQuestCommand>()
                .Map(dest => dest.QuestType, src => Domain.Enum.QuestTypeEnum.Daily);

            config.NewConfig<CreateWeeklyQuestRequest, CreateWeeklyQuestCommand>()
                .Map(dest => dest.QuestType, src => Domain.Enum.QuestTypeEnum.Weekly);

            config.NewConfig<CreateMonthlyQuestRequest, CreateMonthlyQuestCommand>()
                .Map(dest => dest.QuestType, src => Domain.Enum.QuestTypeEnum.Monthly);

            config.NewConfig<CreateSeasonalQuestRequest, CreateSeasonalQuestCommand>()
                .Map(dest => dest.QuestType, src => Domain.Enum.QuestTypeEnum.Seasonal);
        }
    }
}
