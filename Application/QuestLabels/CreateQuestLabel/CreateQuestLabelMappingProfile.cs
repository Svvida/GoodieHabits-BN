using Domain.Models;
using Mapster;

namespace Application.QuestLabels.CreateQuestLabel
{
    public class CreateQuestLabelMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateQuestLabelRequest, CreateQuestLabelCommand>();

            config.NewConfig<QuestLabel, CreateQuestLabelResponse>();
        }
    }
}
