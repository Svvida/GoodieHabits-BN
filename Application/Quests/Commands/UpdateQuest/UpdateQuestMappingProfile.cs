using Mapster;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateQuestRequest, UpdateQuestCommand>();
        }
    }
}
