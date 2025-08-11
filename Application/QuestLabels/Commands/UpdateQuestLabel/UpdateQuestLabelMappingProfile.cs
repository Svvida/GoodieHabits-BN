using Domain.Models;
using Mapster;

namespace Application.QuestLabels.Commands.UpdateQuestLabel
{
    public class UpdateQuestLabelMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateQuestLabelRequest, UpdateQuestLabelCommand>();

            config.NewConfig<QuestLabel, UpdateQuestLabelResponse>();
        }
    }
}
