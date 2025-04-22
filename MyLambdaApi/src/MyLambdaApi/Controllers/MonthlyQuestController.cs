using Application.Dtos.Quests.MonthlyQuest;
using Application.Interfaces.Quests;
using Domain;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLambdaApi.Filters;

namespace MyLambdaApi.Controllers
{
    [ApiController]
    [Route("api/monthly-quests")]
    [Authorize]
    public class MonthlyQuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private static QuestTypeEnum QuestType => QuestTypeEnum.Monthly;

        public MonthlyQuestController(IQuestService questService)
        {
            _questService = questService;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetMonthlyQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
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
        public async Task<ActionResult<IEnumerable<GetMonthlyQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _questService.GetAllUserQuestsByTypeAsync(accountId, QuestType, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateMonthlyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            var createdId = await _questService.CreateUserQuestAsync(createDto, QuestType, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> PatchQuestCompletion(
            int id,
            [FromBody] MonthlyQuestCompletionPatchDto patchDto,
            CancellationToken cancellationToken = default)
        {
            patchDto.Id = id;
            await _questService.UpdateQuestCompletionAsync(patchDto, QuestType, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateMonthlyQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            updateDto.Id = id;
            await _questService.UpdateUserQuestAsync(updateDto, QuestType, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _questService.DeleteQuestAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
