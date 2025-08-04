using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.GetUserLabels
{
    public class GetUserLabelsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserLabelsQuery, GetQuestLabelsResponse>
    {
        public async Task<GetQuestLabelsResponse> Handle(GetUserLabelsQuery request, CancellationToken cancellationToken)
        {
            var labels = await unitOfWork.QuestLabels.GetUserLabelsAsync(request.AccountId, true, cancellationToken).ConfigureAwait(false);
            return mapper.Map<GetQuestLabelsResponse>(labels);
        }
    }
}