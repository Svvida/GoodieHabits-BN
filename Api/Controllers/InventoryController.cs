using Api.Helpers;
using Application.Inventories.Commands.EquipInventoryItem;
using Application.Inventories.Commands.UnequipInventoryItem;
using Application.Inventories.Commands.UseInventoryItem;
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

        [HttpPost("items/{inventoryId}/equip")]
        public async Task<IActionResult> EquipInventoryItem(int inventoryId, CancellationToken cancellationToken)
        {
            var command = new EquipInventoryItemCommand(
                JwtHelpers.GetCurrentUserProfileId(User),
                inventoryId);
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost("items/{inventoryId}/unequip")]
        public async Task<IActionResult> UnequipInventoryItem(int inventoryId, CancellationToken cancellationToken)
        {
            var command = new UnequipInventoryItemCommand(
                JwtHelpers.GetCurrentUserProfileId(User),
                inventoryId);
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }

        [HttpPost("items/{inventoryId}/use")]
        public async Task<IActionResult> UseInventoryItem(int inventoryId, CancellationToken cancellationToken)
        {
            var command = new UseInventoryItemCommand(
                JwtHelpers.GetCurrentUserProfileId(User),
                inventoryId);
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
