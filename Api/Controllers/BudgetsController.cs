using Api.Helpers;
using Application.Finance.Budgets.Commands.CreateBudget;
using Application.Finance.Budgets.Commands.DeleteBudget;
using Application.Finance.Budgets.Commands.UpdateBudget;
using Application.Finance.Budgets.Dtos;
using Application.Finance.Budgets.Queries.GetBudgets;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/budgets")]
    public class BudgetsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAsync(
            [FromQuery] int year, [FromQuery] int? month = null, CancellationToken cancellationToken = default)
        {
            var query = new GetBudgetsQuery(User.GetCurrentUserProfileId(), year, month);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<BudgetDto>> CreateAsync(
            [FromBody] CreateBudgetRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateBudgetCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<BudgetDto>> UpdateAsync(
            int id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateBudgetCommand>(request) with
            {
                BudgetId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteBudgetCommand(id, User.GetCurrentUserProfileId());
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
