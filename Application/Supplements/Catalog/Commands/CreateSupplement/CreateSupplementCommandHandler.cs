using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.CreateSupplement
{
    public class CreateSupplementCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateSupplementCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(CreateSupplementCommand request, CancellationToken cancellationToken)
        {
            var exists = await unitOfWork.Supplements
                .ExistsByNameAsync(request.UserProfileId, request.Name, null, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
                throw new ConflictException($"A supplement named '{request.Name.Trim()}' already exists.");

            var supplement = Supplement.Create(
                request.UserProfileId,
                request.Name,
                request.Unit,
                request.DefaultAmount,
                request.Note,
                request.Color,
                request.Icon);

            await unitOfWork.Supplements.AddAsync(supplement, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
