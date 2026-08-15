using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.UpdateSupplement
{
    /// <summary>
    /// Edits the supplement's own fields.
    /// <para>
    /// Changing the <c>Unit</c> is allowed and deliberately does <b>not</b> convert anything: slot amounts and
    /// already logged doses keep their numbers and are simply read in the new unit. Same posture as the
    /// currency and weight-unit settings — silent conversion of historical records is the worse failure.
    /// </para>
    /// </summary>
    public class UpdateSupplementCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateSupplementCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(UpdateSupplementCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            var nameTaken = await unitOfWork.Supplements
                .ExistsByNameAsync(request.UserProfileId, request.Name, request.SupplementId, cancellationToken)
                .ConfigureAwait(false);

            if (nameTaken)
                throw new ConflictException($"A supplement named '{request.Name.Trim()}' already exists.");

            supplement.Rename(request.Name);
            supplement.UpdateUnit(request.Unit);
            supplement.UpdateDefaultAmount(request.DefaultAmount);
            supplement.UpdateNote(request.Note);
            supplement.UpdateAppearance(request.Color, request.Icon);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
