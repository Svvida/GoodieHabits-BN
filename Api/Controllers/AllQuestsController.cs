using Application.Dtos.Quests.QuestMetadata;
using Application.Interfaces.Quests;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/all-quests")]
    public class AllQuestsController : ControllerBase
    {
        private readonly IQuestMetadataService _questMetadataService;

        public AllQuestsController(IQuestMetadataService questMetadataService)
        {
            _questMetadataService = questMetadataService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetQuestMetadataDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            return Ok(await _questMetadataService.GetAllQuestsAsync(accountId, cancellationToken));
        }
    }
}
