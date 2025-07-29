using Api.Helpers;
using Application.Accounts.Queries.GetWithProfile;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using MediatR;
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
        private readonly ISender _sender;

        public AccountController(IAccountService accountService, ISender sender)
        {
            _accountService = accountService;
            _sender = sender;
        }

        [HttpGet("accounts/me")]
        public async Task<ActionResult<GetAccountWithProfileDto>> GetAccount(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetAccountWithProfileQuery(accountId);

            var account = await _sender.Send(query, cancellationToken);

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
