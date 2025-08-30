using Application.QuestLabels.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.QuestLabels.Queries.GetUserLabels
{
    public class GetUserLabelsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserLabelsQuery, IEnumerable<QuestLabelDto>>
    {
        public async Task<IEnumerable<QuestLabelDto>> Handle(GetUserLabelsQuery request, CancellationToken cancellationToken)
        {
            var labels = await unitOfWork.QuestLabels.GetUserLabelsAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);
            return mapper.Map<IEnumerable<QuestLabelDto>>(labels);
        }
    }
}