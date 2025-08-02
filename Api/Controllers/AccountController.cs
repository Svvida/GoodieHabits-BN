using Api.Helpers;
using Application.Accounts.Commands.ChangePassword;
using Application.Accounts.Commands.DeleteAccount;
using Application.Accounts.Commands.UpdateAccount;
using Application.Accounts.Commands.WipeoutData;
using Application.Accounts.Queries.GetWithProfile;
using Application.Dtos.Accounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AccountController(ISender sender) : ControllerBase
    {
        [HttpGet("accounts/me")]
        public async Task<ActionResult<GetAccountWithProfileDto?>> GetAccount(CancellationToken cancellationToken = default)
        {
            var query = new GetAccountWithProfileQuery(JwtHelpers.GetCurrentUserId(User));
            var account = await sender.Send(query, cancellationToken);
            return Ok(account);
        }

        [HttpPut("accounts/me")]
        public async Task<IActionResult> UpdateAccount(
            UpdateAccountCommand command,
            CancellationToken cancellationToken = default)
        {
            command.AccountId = JwtHelpers.GetCurrentUserId(User);

            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPut("accounts/me/password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            command.AccountId = JwtHelpers.GetCurrentUserId(User);

            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("accounts/me")]
        public async Task<IActionResult> DeleteAccount(
            DeleteAccountCommand command,
            CancellationToken cancellationToken = default)
        {
            command.AccountId = JwtHelpers.GetCurrentUserId(User);
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/me/wipeout-data")]
        public async Task<IActionResult> WipeoutAccountData(
            WipeoutDataCommand command,
            CancellationToken cancellationToken = default)
        {
            command.AccountId = JwtHelpers.GetCurrentUserId(User);
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
