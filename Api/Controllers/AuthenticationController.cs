using Application.Auth.Login;
using Application.Auth.RefreshAccessToken;
using Application.Auth.Register;
using Application.Dtos.Auth;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpPost]
        [Route("auth/login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<LoginCommand>(request);
            var loginResponse = await sender.Send(command, cancellationToken);
            return Ok(loginResponse);
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<AuthResponseDto>> CreateAccount(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            var timeZoneId = Request.Headers.TryGetValue("x-time-zone", out var tzHeader) ? tzHeader.ToString() : null;
            var command = mapper.Map<RegisterCommand>(request) with { TimeZoneId = timeZoneId };
            var registerResponse = await sender.Send(command, cancellationToken);
            return Created(nameof(CreateAccount), registerResponse);
        }

        [HttpPost]
        [Route("auth/refresh-token")]
        public async Task<ActionResult<RefreshResponseDto>> RefreshToken(
            [FromBody] RefreshAccessTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var timeZone = Request.Headers.TryGetValue("x-time-zone", out var tzHeader) ? tzHeader.ToString() : null;

            var command = new RefreshAccessTokenCommand(request.RefreshToken, timeZone);
            var accessToken = await sender.Send(command, cancellationToken);

            return Ok(accessToken);
        }
    }
}
