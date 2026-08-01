using Api.Helpers;
using Application.Finance.RecurringTransactions.Commands.CreateRecurringTransaction;
using Application.Finance.RecurringTransactions.Commands.DeleteRecurringTransaction;
using Application.Finance.RecurringTransactions.Commands.UpdateRecurringTransaction;
using Application.Finance.RecurringTransactions.Dtos;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/recurring-transactions")]
    public class RecurringTransactionsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecurringTransactionDto>>> GetAsync(CancellationToken cancellationToken = default)
        {
            var query = new Application.Finance.RecurringTransactions.Queries.GetRecurringTransactions
                .GetRecurringTransactionsQuery(User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<RecurringTransactionDto>> CreateAsync(
            [FromBody] CreateRecurringTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateRecurringTransactionCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Partial update — omitted fields are left unchanged.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<RecurringTransactionDto>> UpdateAsync(
            int id, [FromBody] UpdateRecurringTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateRecurringTransactionCommand>(request) with
            {
                RecurringTransactionId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        /// <summary>Deletes the template. Transactions already generated from it are kept, unlinked.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteRecurringTransactionCommand(id, User.GetCurrentUserProfileId());
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
