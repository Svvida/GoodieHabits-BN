using Application.Finance.Categories.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Categories.Commands.CreateFinanceCategory
{
    public class CreateFinanceCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateFinanceCategoryCommand, FinanceCategoryDto>
    {
        public async Task<FinanceCategoryDto> Handle(CreateFinanceCategoryCommand request, CancellationToken cancellationToken)
        {
            FinanceCategory category;

            if (request.ParentCategoryId is int parentId)
            {
                var parent = await unitOfWork.FinanceCategories
                    .GetAssignableByIdAsync(parentId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Parent category with ID {parentId} not found.");

                if (!parent.IsMain)
                    throw new ConflictException("Categories can only be nested one level deep.");

                await EnsureNameUniqueAsync(request.Name, request.UserProfileId, parent.Type, parent.Id, cancellationToken).ConfigureAwait(false);

                category = FinanceCategory.CreateSub(request.UserProfileId, parent, request.Name, request.Color, request.Icon);
            }
            else
            {
                await EnsureNameUniqueAsync(request.Name, request.UserProfileId, request.Type, null, cancellationToken).ConfigureAwait(false);

                category = FinanceCategory.CreateMain(request.UserProfileId, request.Name, request.Type, request.Color, request.Icon, request.IsSavings);
            }

            await unitOfWork.FinanceCategories.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<FinanceCategoryDto>(category);
        }

        private async Task EnsureNameUniqueAsync(
            string name,
            int userProfileId,
            FinanceTransactionTypeEnum type,
            int? parentCategoryId,
            CancellationToken cancellationToken)
        {
            var isUnique = await unitOfWork.FinanceCategories
                .IsNameUniqueUnderParentAsync(name.Trim(), userProfileId, type, parentCategoryId, null, cancellationToken)
                .ConfigureAwait(false);

            if (!isUnique)
                throw new ConflictException($"A category named '{name.Trim()}' already exists at this level.");
        }
    }
}
