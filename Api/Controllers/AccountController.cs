using Api.Helpers;
using Application.Accounts.Commands.ChangePassword;
using Application.Accounts.Commands.DeleteAccount;
using Application.Accounts.Commands.UpdateAccount;
using Application.Accounts.Commands.WipeoutData;
using Application.Accounts.Queries.GetWithProfile;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AccountController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet("accounts/me")]
        public async Task<ActionResult<GetAccountWithProfileResponse>> GetAccount(CancellationToken cancellationToken = default)
        {
            var query = new GetAccountWithProfileQuery(JwtHelpers.GetCurrentUserId(User));
            var account = await sender.Send(query, cancellationToken);
            return Ok(account);
        }

        [HttpPut("accounts/me")]
        public async Task<IActionResult> UpdateAccount(
            UpdateAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateAccountCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPut("accounts/me/password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<ChangePasswordCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("accounts/me")]
        public async Task<IActionResult> DeleteAccount(
            DeleteAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<DeleteAccountCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/me/wipeout-data")]
        public async Task<IActionResult> WipeoutAccountData(
            WipeoutDataRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<WipeoutDataCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
