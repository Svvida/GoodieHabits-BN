using Application.QuestLabels.Dtos;
using Domain.Models;
using Mapster;

namespace Application.QuestLabels.Mappings
{
    public class QuestLabelMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestLabel, QuestLabelDto>();

            config.NewConfig<Quest_QuestLabel, QuestLabelDto>()
                .Map(dest => dest.Id, src => src.QuestLabel.Id)
                .Map(dest => dest.Value, src => src.QuestLabel.Value)
                .Map(dest => dest.BackgroundColor, src => src.QuestLabel.BackgroundColor);
        }
    }
}
