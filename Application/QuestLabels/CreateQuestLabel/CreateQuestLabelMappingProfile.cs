using AutoMapper;
using Domain.Models;

namespace Application.QuestLabels.CreateQuestLabel
{
    public class CreateQuestLabelMappingProfile : Profile
    {
        public CreateQuestLabelMappingProfile()
        {
            CreateMap<CreateQuestLabelRequest, CreateQuestLabelCommand>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());

            CreateMap<QuestLabel, CreateQuestLabelResponse>();
        }
    }
}
