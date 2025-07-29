using Application.Dtos.UserProfile;
using Application.UserProfiles.Nickname;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class NicknameController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpGet("nickname/random")]
        public async Task<ActionResult<GetNicknameDto>> GenerateUniqueNickname(CancellationToken cancellationToken = default)
        {
            var randomNickname = await _sender.Send(new GetRandomNicknameQuery(), cancellationToken);

            return Ok(randomNickname);
        }
    }
}
