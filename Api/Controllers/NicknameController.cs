using Application.Nicknames.GetRandom;
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

        [HttpGet("nickname/random")]
        public async Task<ActionResult<GetRandomNicknameResponse>> GenerateUniqueNickname(CancellationToken cancellationToken = default)
        {
            var randomNickname = await sender.Send(new GetRandomNicknameQuery(), cancellationToken);

            return Ok(randomNickname);
        }
    }
}
