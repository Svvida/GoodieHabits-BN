using Api.Filters;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Interfaces.Quests;
using Domain;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/seasonal-quests")]
    [Authorize]
    public class SeasonalQuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private static QuestTypeEnum QuestType => QuestTypeEnum.Seasonal;

        public SeasonalQuestController(IQuestService questService)
        {
            _questService = questService;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetSeasonalQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _questService.GetUserQuestByIdAsync(id, QuestType, cancellationToken);

            if (quest is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Quest not found",
                    Detail = $"Quest with ID {id} was not found"
                });
            }

            return Ok(quest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonalQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _questService.GetAllUserQuestsByTypeAsync(accountId, QuestType, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<GetSeasonalQuestDto>> Create(
            [FromBody] CreateSeasonalQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            var createdQuest = await _questService.CreateUserQuestAsync(createDto, QuestType, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdQuest.Id }, createdQuest);
        }

        [HttpPatch("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetSeasonalQuestDto>> PatchQuestCompletion(
            int id,
            [FromBody] SeasonalQuestCompletionPatchDto patchDto,
            CancellationToken cancellationToken = default)
        {
            patchDto.Id = id;
            var updatedQuest = await _questService.UpdateQuestCompletionAsync(patchDto, QuestType, cancellationToken);
            return Ok(updatedQuest);

        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetSeasonalQuestDto>> Update(
            int id,
            [FromBody] UpdateSeasonalQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                return Unauthorized("Invalid access token: missing account identifier.");

            updateDto.AccountId = accountId;
            updateDto.Id = id;
            var updatedQuest = await _questService.UpdateUserQuestAsync(updateDto, QuestType, cancellationToken);
            return Ok(updatedQuest);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                return Unauthorized("Invalid access token: missing account identifier.");

            await _questService.DeleteQuestAsync(id, QuestType, accountId, cancellationToken);
            return NoContent();
        }
    }
}
