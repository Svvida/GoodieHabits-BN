using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.SetSupplementActive
{
    /// <summary>
    /// The retire path. Deactivating drops the supplement (and its slots) off the checklist while keeping every
    /// dose ever logged — which is why deleting one with intakes is refused and this exists instead.
    /// </summary>
    public class SetSupplementActiveCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<SetSupplementActiveCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(SetSupplementActiveCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            supplement.SetActive(request.IsActive);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
