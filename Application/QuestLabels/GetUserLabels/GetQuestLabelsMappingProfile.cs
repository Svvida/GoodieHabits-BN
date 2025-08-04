using AutoMapper;
using Domain.Models;

namespace Application.QuestLabels.GetUserLabels
{
    public class GetQuestLabelsMappingProfile : Profile
    {
        public GetQuestLabelsMappingProfile()
        {
            CreateMap<IEnumerable<QuestLabel>, GetQuestLabelsResponse>()
                .ForCtorParam(nameof(GetQuestLabelsResponse.QuestLabels), opt => opt.MapFrom(src => src));
        }
    }
}
