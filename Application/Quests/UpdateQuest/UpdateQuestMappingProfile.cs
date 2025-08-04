using AutoMapper;

namespace Application.Quests.UpdateQuest
{
    public class UpdateQuestMappingProfile : Profile
    {
        public UpdateQuestMappingProfile()
        {
            CreateMap<UpdateOneTimeQuestRequest, UpdateOneTimeQuestCommand>();

            CreateMap<UpdateDailyQuestRequest, UpdateDailyQuestCommand>();

            CreateMap<UpdateWeeklyQuestRequest, UpdateWeeklyQuestCommand>();

            CreateMap<UpdateMonthlyQuestRequest, UpdateMonthlyQuestCommand>();

            CreateMap<UpdateSeasonalQuestRequest, UpdateSeasonalQuestCommand>();
        }
    }
}
