using Application.Dtos.Labels;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.QuestLabels.Queries.GetUserLabels
{
    public class GetUserLabelsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserLabelsQuery, IEnumerable<GetQuestLabelDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<GetQuestLabelDto>> Handle(GetUserLabelsQuery request, CancellationToken cancellationToken)
        {
            var labels = await _unitOfWork.QuestLabels.GetUserLabelsAsync(request.AccountId, true, cancellationToken).ConfigureAwait(false);
            return _mapper.Map<IEnumerable<GetQuestLabelDto>>(labels);
        }
    }
}