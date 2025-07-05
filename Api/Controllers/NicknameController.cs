using Application.Dtos.UserProfile;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class NicknameController : ControllerBase
    {
        private readonly INicknameGeneratorService _nicknameGeneratorService;

        public NicknameController(INicknameGeneratorService nicknameGeneratorService)
        {
            _nicknameGeneratorService = nicknameGeneratorService;
        }

        [HttpGet("nickname/random")]
        public async Task<ActionResult<GetNicknameDto>> GenerateUniqueNickname(CancellationToken cancellationToken = default)
        {
            var randomNickname = await _nicknameGeneratorService.GenerateUniqueNicknameAsync(cancellationToken).ConfigureAwait(false);

            return new GetNicknameDto
            {
                Nickname = randomNickname
            };
        }
    }
}
