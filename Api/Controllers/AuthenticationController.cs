using Application.Accounts.Commands.UpdateTimeZoneIfChanged;
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
    public class AuthenticationController(IAuthService authService, ISender sender) : ControllerBase
    {
        [HttpPost]
        [Route("auth/login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            var authResponse = await authService.LoginAsync(loginDto, cancellationToken);
            return Ok(authResponse);
        }


        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<AuthResponseDto>> CreateAccount(
            [FromBody] CreateAccountDto createDto,
            CancellationToken cancellationToken = default)
        {
            var authResponse = await authService.RegisterAsync(createDto, cancellationToken);
            return Created(nameof(CreateAccount), authResponse);
        }

        [HttpPost]
        [Route("auth/refresh-token")]
        public async Task<ActionResult<RefreshResponseDto>> RefreshToken(
            [FromBody] RefreshRequestDto refreshRequestDto,
            CancellationToken cancellationToken = default)
        {
            var refreshResponse = await authService.RefreshAccessTokenAsync(refreshRequestDto.RefreshToken, cancellationToken);

            var timeZone = Request.Headers.TryGetValue("x-time-zone", out var tzHeader) ? tzHeader.ToString() : null;
            var command = new UpdateTimeZoneIfChangedCommand(refreshResponse.AccountId, timeZone);
            await sender.Send(command, cancellationToken);

            return Ok(refreshResponse);
        }
    }
}
