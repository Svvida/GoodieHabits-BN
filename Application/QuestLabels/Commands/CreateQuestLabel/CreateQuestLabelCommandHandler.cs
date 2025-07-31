using Application.QuestLabels.Queries.GetUserLabels;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public class CreateQuestLabelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateQuestLabelCommand, GetQuestLabelDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<GetQuestLabelDto> Handle(CreateQuestLabelCommand request, CancellationToken cancellationToken)
        {
            var createDto = request.CreateDto;

            var label = QuestLabel.Create(
                accountId: createDto.AccountId,
                value: createDto.Value,
                backgroundColor: createDto.BackgroundColor);

            await _unitOfWork.QuestLabels.AddAsync(label, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<GetQuestLabelDto>(label);
        }
    }
}
