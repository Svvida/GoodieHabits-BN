using Mapster;

namespace Application.Quests.UpdateQuestCompletion
{
    public class UpdateQuestCompletionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateQuestCompletionRequest, UpdateQuestCompletionCommand>();
        }
    }
}
