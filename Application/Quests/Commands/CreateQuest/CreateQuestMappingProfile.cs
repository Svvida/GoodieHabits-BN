using Mapster;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateQuestRequest, CreateQuestCommand>();
        }
    }
}
