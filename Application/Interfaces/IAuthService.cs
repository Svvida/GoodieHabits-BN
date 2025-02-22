using Application.Dtos;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    }
}
