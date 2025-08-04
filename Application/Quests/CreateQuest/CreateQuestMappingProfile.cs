using AutoMapper;

namespace Application.Quests.CreateQuest
{
    public class CreateQuestMappingProfile : Profile
    {
        public CreateQuestMappingProfile()
        {
            CreateMap<CreateOneTimeQuestRequest, CreateOneTimeQuestCommand>();

            CreateMap<CreateDailyQuestRequest, CreateDailyQuestCommand>();

            CreateMap<CreateWeeklyQuestRequest, CreateWeeklyQuestCommand>();

            CreateMap<CreateMonthlyQuestRequest, CreateMonthlyQuestCommand>();

            CreateMap<CreateSeasonalQuestRequest, CreateSeasonalQuestCommand>();
        }
    }
}
