using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Inventories.Queries.GetUserInventoryItems
{
    public class GetUserInventoryItemsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserInventoryItemsQuery, List<UserInventoryItemDto>>
    {
        public async Task<List<UserInventoryItemDto>> Handle(GetUserInventoryItemsQuery request, CancellationToken cancellationToken)
        {
            var userInventoryItems = await unitOfWork.UserInventories.GetUserInventoryItemsAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);
            return mapper.Map<List<UserInventoryItemDto>>(userInventoryItems);
        }
    }
}
