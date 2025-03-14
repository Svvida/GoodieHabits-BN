using Application.Dtos.Accounts;
using Application.Interfaces;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        //[HttpGet("accounts")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetAccountDto>))]
        //public async Task<ActionResult<IEnumerable<GetAccountDto>>> GetAccounts()
        //{
        //    var accounts = await _accountService.GetAllAccountsAsync();
        //    return Ok(accounts);
        //}

        [HttpGet("accounts/me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAccountDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetAccountDto>> GetAccount()
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Account not found",
                    Detail = $"Account with ID {accountId} was not found"
                });
            }
            return Ok(account);
        }

        [HttpPatch("accounts/me")]
        public async Task<IActionResult> PatchAccount(
            PatchAccountDto patchDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            if (patchDto.Nickname is not null)
                patchDto.Nickname = patchDto.Nickname.Trim();

            if (patchDto.Email is not null)
                patchDto.Email = patchDto.Email.Trim();

            await _accountService.PatchAccountAsync(accountId, patchDto, cancellationToken);
            return NoContent();
        }
    }
}
