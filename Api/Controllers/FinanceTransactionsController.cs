using Api.Helpers;
using Application.Common.Dtos;
using Application.Finance.Transactions.Commands.CreateTransaction;
using Application.Finance.Transactions.Commands.DeleteTransaction;
using Application.Finance.Transactions.Commands.UpdateTransaction;
using Application.Finance.Transactions.Dtos;
using Application.Finance.Transactions.Queries.GetTransactionById;
using Application.Finance.Transactions.Queries.GetTransactions;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/transactions")]
    public class FinanceTransactionsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<TransactionDto>>> GetAsync(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] FinanceTransactionTypeEnum? type = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetTransactionsQuery(User.GetCurrentUserProfileId(), from, to, type, categoryId, page, pageSize);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TransactionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var query = new GetTransactionByIdQuery(id, User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateAsync(
            [FromBody] CreateTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateTransactionCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TransactionDto>> UpdateAsync(
            int id, [FromBody] UpdateTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateTransactionCommand>(request) with
            {
                TransactionId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteTransactionCommand(id, User.GetCurrentUserProfileId());
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
