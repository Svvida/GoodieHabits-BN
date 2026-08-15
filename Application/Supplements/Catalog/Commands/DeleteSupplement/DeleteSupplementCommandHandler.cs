using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Supplements.Catalog.Commands.DeleteSupplement
{
    /// <summary>
    /// Deletes a supplement and its schedule.
    /// <para>
    /// <b>Blocked once anything has been logged against it</b> — deleting would erase the record of doses the
    /// user actually took, which is the one thing this module exists to keep. The conflict points at
    /// deactivation instead, which achieves what the user wanted (it off the checklist) without the loss.
    /// The FK is <c>Restrict</c> underneath, so the database agrees.
    /// </para>
    /// </summary>
    public class DeleteSupplementCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteSupplementCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteSupplementCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            var hasIntakes = await unitOfWork.Supplements
                .HasIntakesAsync(request.SupplementId, cancellationToken)
                .ConfigureAwait(false);

            if (hasIntakes)
                throw new ConflictException(
                    "This supplement has logged intakes and cannot be deleted. Deactivate it instead to remove it from the checklist.");

            unitOfWork.Supplements.Remove(supplement);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
