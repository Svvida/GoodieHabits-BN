using Api.Filters;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost]
        [Route("auth/login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]  // Returns JWT Token
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Validation Errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Invalid credentials
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            var trimmedData = new LoginDto
            {
                Login = loginDto.Login.Trim(),
                Password = loginDto.Password.Trim()
            };

            var authResponse = await _authService.LoginAsync(trimmedData, cancellationToken);
            return Ok(authResponse);
        }


        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<ActionResult<AuthResponseDto>> CreateAccount(
            [FromBody] CreateAccountDto createDto,
            CancellationToken cancellationToken = default)
        {
            var trimmedData = new CreateAccountDto
            {
                Email = createDto.Email.Trim(),
                Password = createDto.Password.Trim()
            };
            var authResponse = await _authService.RegisterAsync(trimmedData, cancellationToken);
            return Created(nameof(CreateAccount), authResponse);
        }

        [HttpPost]
        [Route("auth/refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshResponseDto))]  // Returns JWT Token
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Validation Errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Invalid credentials
        [ServiceFilter(typeof(TimeZoneUpdateFilter))]
        public async Task<ActionResult<RefreshResponseDto>> RefreshToken(
            [FromBody] RefreshRequestDto refreshRequestDto,
            CancellationToken cancellationToken = default)
        {
            var refreshResponse = await _authService.RefreshAccessTokenAsync(refreshRequestDto.RefreshToken.Trim(), cancellationToken);
            return Ok(refreshResponse);
        }
    }
}
