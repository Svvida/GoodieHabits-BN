using Application.Finance.Categories.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Categories.Commands.UpdateFinanceCategory
{
    public class UpdateFinanceCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateFinanceCategoryCommand, FinanceCategoryDto>
    {
        public async Task<FinanceCategoryDto> Handle(UpdateFinanceCategoryCommand request, CancellationToken cancellationToken)
        {
            // Owned-only lookup: system categories are excluded, so they surface as 404 (not editable).
            var category = await unitOfWork.FinanceCategories
                .GetOwnedByIdAsync(request.CategoryId, request.UserProfileId, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Category with ID {request.CategoryId} not found.");

            var isUnique = await unitOfWork.FinanceCategories
                .IsNameUniqueUnderParentAsync(request.Name.Trim(), request.UserProfileId, category.Type, category.ParentCategoryId, category.Id, cancellationToken)
                .ConfigureAwait(false);

            if (!isUnique)
                throw new ConflictException($"A category named '{request.Name.Trim()}' already exists at this level.");

            category.Rename(request.Name);
            category.UpdateColor(request.Color);
            category.UpdateIcon(request.Icon);

            // IsSavings is owned by the main category; sub-categories inherit it, so their incoming value is ignored
            // and a change on a main cascades down to keep the branch consistent.
            if (category.IsMain && category.IsSavings != request.IsSavings)
            {
                category.UpdateIsSavings(request.IsSavings);

                var subCategories = await unitOfWork.FinanceCategories
                    .GetOwnedSubCategoriesAsync(category.Id, request.UserProfileId, cancellationToken).ConfigureAwait(false);

                foreach (var subCategory in subCategories)
                    subCategory.UpdateIsSavings(request.IsSavings);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<FinanceCategoryDto>(category);
        }
    }
}
