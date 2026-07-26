using Application.Finance.Categories.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Categories.Queries.GetUserCategoryTree
{
    public class GetUserCategoryTreeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetUserCategoryTreeQuery, IEnumerable<FinanceCategoryDto>>
    {
        public async Task<IEnumerable<FinanceCategoryDto>> Handle(GetUserCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            var categories = await unitOfWork.FinanceCategories
                .GetUserCategoryTreeAsync(request.UserProfileId, true, cancellationToken).ConfigureAwait(false);

            if (request.Type.HasValue)
                categories = categories.Where(c => c.Type == request.Type.Value);

            // Flat -> tree. Map each category, then nest subs under their parent (system + user's own share the set).
            var dtos = categories.Select(mapper.Map<FinanceCategoryDto>).ToList();
            var byId = dtos.ToDictionary(d => d.Id);

            var roots = new List<FinanceCategoryDto>();
            foreach (var dto in dtos)
            {
                if (dto.ParentCategoryId is int parentId && byId.TryGetValue(parentId, out var parent))
                    parent.SubCategories.Add(dto);
                else
                    roots.Add(dto);
            }

            foreach (var root in roots)
                root.SubCategories = root.SubCategories.OrderBy(s => s.Name).ToList();

            return roots.OrderBy(r => r.Type).ThenBy(r => r.Name).ToList();
        }
    }
}
