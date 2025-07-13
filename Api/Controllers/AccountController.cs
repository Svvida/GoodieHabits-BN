using Api.Helpers;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
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

        [HttpGet("accounts/me")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAccountDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<GetAccountDto>> GetAccount()
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

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
            var accountId = JwtHelpers.GetCurrentUserId(User);

            await _accountService.UpdateAccountAsync(accountId, patchDto, cancellationToken);
            return NoContent();
        }

        [HttpPut("accounts/me/password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto changePasswordDto,
            CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            await _accountService.ChangePasswordAsync(accountId, changePasswordDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("accounts/me")]
        public async Task<IActionResult> DeleteAccount(
            PasswordConfirmationDto passwordConfirmationDto,
            CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);
            await _accountService.DeleteAccountAsync(accountId, passwordConfirmationDto, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/me/wipeout-data")]
        public async Task<IActionResult> WipeoutAccountData(
            PasswordConfirmationDto authRequestDto,
            CancellationToken cancellationToken = default)
        {
            authRequestDto.AccountId = JwtHelpers.GetCurrentUserId(User);
            await _accountService.WipeoutAccountDataAsync(authRequestDto, cancellationToken);
            return NoContent();
        }
    }
}
