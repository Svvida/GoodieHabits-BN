using Api.Helpers;
using Application.Inventories.Queries.GetUserInventoryItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController(ISender sender) : ControllerBase
    {
        [HttpGet("items")]
        public async Task<ActionResult<List<UserInventoryItemDto>>> GetUserInventoryItems(CancellationToken cancellationToken)
        {
            var query = new GetUserInventoryItemsQuery(JwtHelpers.GetCurrentUserProfileId(User));
            var userInventoryItems = await sender.Send(query, cancellationToken).ConfigureAwait(false);
            return Ok(userInventoryItems);
        }
    }
}
