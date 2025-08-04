using Application.Accounts.Dtos;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using Domain;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyLambdaApi.Controllers
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

        [HttpGet("accounts/me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAccountWithProfileDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetAccountWithProfileDto>> GetAccount()
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var account = await _accountService.GetAccountWithProfileInfoAsync(accountId);
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

        [HttpPut("accounts/me")]
        public async Task<IActionResult> UpdateAccount(
            UpdateAccountDto patchDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            await _accountService.UpdateAccountAsync(accountId, patchDto, cancellationToken);
            return NoContent();
        }

        [HttpPut("accounts/me/password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto changePasswordDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            await _accountService.ChangePasswordAsync(accountId, changePasswordDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("accounts/me")]
        public async Task<IActionResult> DeleteAccount(
            PasswordConfirmationDto deleteAccountDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");
            await _accountService.DeleteAccountAsync(accountId, deleteAccountDto, cancellationToken);
            return NoContent();
        }
    }
}
