using Api.Filters;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using Domain;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/one-time-quests")]
    [ApiController]
    [Authorize]
    public class OneTimeQuestController : ControllerBase
    {
        private readonly IOneTimeQuestService _service;

        public OneTimeQuestController(
            IOneTimeQuestService oneTimeQuestService)
        {
            _service = oneTimeQuestService;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetOneTimeQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _service.GetUserQuestByIdAsync(id, cancellationToken);

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
        public async Task<ActionResult<IEnumerable<GetOneTimeQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _service.GetAllUserQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateOneTimeQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            var createdId = await _service.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> UpdateUserQuestPartial(
            int id,
            [FromBody] PatchOneTimeQuestDto patchDto,
            CancellationToken cancellationToken = default)
        {
            await _service.PatchUserQuestAsync(id, patchDto, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> UpdateUserQuest(
            int id,
            [FromBody] UpdateOneTimeQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            await _service.UpdateUserQuestAsync(id, updateDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteUserQuestAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
