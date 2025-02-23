using Application.Dtos.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]  // Returns JWT Token
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Validation Errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Invalid credentials
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _authService.LoginAsync(loginDto);
            return Ok(authResponse);
        }

        [HttpPost]
        [Route("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshResponseDto))]  // Returns JWT Token
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Validation Errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Invalid credentials
        public async Task<ActionResult<RefreshResponseDto>> RefreshToken([FromBody] RefreshRequestDto refreshRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refreshResponse = await _authService.RefreshAccessTokenAsync(refreshRequestDto.RefreshToken);
            return Ok(refreshResponse);
        }
    }
}
