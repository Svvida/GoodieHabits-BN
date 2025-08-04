using AutoMapper;
using Domain.Models;

namespace Application.QuestLabels.UpdateQuestLabel
{
    public class UpdateQuestLabelMappingProfile : Profile
    {
        public UpdateQuestLabelMappingProfile()
        {
            CreateMap<UpdateQuestLabelRequest, UpdateQuestLabelCommand>()
                .ForMember(dest => dest.LabelId, opt => opt.Ignore())
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());

            CreateMap<QuestLabel, UpdateQuestLabelResponse>();
        }
    }
}
