using Api.Helpers;
using Application.Common.Sorting;
using Application.Shop.Queries.GetItemsWithUserContext;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/shop")]
    public class ShopController(ISender sender) : ControllerBase
    {
        [HttpGet("items")]
        public async Task<ActionResult<List<ShopItemDto>>> GetAvailableItems(
            [FromQuery] ShopItemSortProperty sortBy = ShopItemSortProperty.Id,
            [FromQuery] SortOrder sortOrder = SortOrder.Asc,
            [FromQuery] ShopItemsCategoryEnum? category = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetItemsWithUserContextQuery(
                JwtHelpers.GetCurrentUserProfileId(User),
                category,
                sortBy,
                sortOrder);
            var items = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(items);
        }
    }
}
