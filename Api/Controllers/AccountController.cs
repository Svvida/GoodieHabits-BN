using Application.Dtos.Accounts;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetAccountDto>))]
        public async Task<ActionResult<IEnumerable<GetAccountDto>>> GetAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("accounts/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAccountDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetAccountDto?>> GetAccount(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Account not found",
                    Detail = $"Account with ID {id} was not found"
                });
            }
            return Ok(account);
        }
    }
}
