using Application.Auth.Commands.Login;
using Application.Auth.Commands.RefreshAccessToken;
using Application.Auth.Commands.Register;
using MapsterMapper;
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
        public async Task<ActionResult<LoginResponse>> Login(
            LoginCommand request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<LoginCommand>(request);
            var loginResponse = await sender.Send(command, cancellationToken);
            return Ok(loginResponse);
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<RegisterResponse>> CreateAccount(
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
        public async Task<ActionResult<RefreshAccessTokenResponse>> RefreshToken(
            [FromBody] RefreshAccessTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            var timeZoneId = Request.Headers.TryGetValue("x-time-zone", out var tzHeader) ? tzHeader.ToString() : null;
            var command = mapper.Map<RefreshAccessTokenCommand>(request) with { TimeZoneId = timeZoneId };
            var accessToken = await sender.Send(command, cancellationToken);
            return Ok(accessToken);
        }
    }
}
