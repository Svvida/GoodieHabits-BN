using Application.Dtos.Auth;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
        public Task<RefreshResponseDto> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
