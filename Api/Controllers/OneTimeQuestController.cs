using Api.Filters;
using Api.Helpers;
using Application.Commands;
using Application.Dtos.Quests;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/one-time-quests")]
    [ApiController]
    [Authorize]
    public class OneTimeQuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private readonly ISender _sender;
        private static QuestTypeEnum QuestType => QuestTypeEnum.OneTime;

        public OneTimeQuestController(IQuestService questService, ISender sender)
        {
            _questService = questService;
            _sender = sender;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetOneTimeQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
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
        public async Task<ActionResult<IEnumerable<GetOneTimeQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var quests = await _questService.GetAllUserQuestsByTypeAsync(accountId, QuestType, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<GetOneTimeQuestDto>> Create(
            [FromBody] CreateOneTimeQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            createDto.AccountId = JwtHelpers.GetCurrentUserId(User);

            var createdQuest = await _questService.CreateUserQuestAsync(createDto, QuestType, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdQuest.Id }, createdQuest);
        }

        [HttpPatch("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetOneTimeQuestDto>> PatchQuestCompletion(
            int id,
            [FromBody] QuestCompletionPatchDto patchDto,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateQuestCompletionCommand(
                id,
                patchDto.IsCompleted,
                QuestType);
            await _sender.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetOneTimeQuestDto>> UpdateUserQuest(
            int id,
            [FromBody] UpdateOneTimeQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            updateDto.AccountId = JwtHelpers.GetCurrentUserId(User);
            updateDto.Id = id;
            var updatedQuest = await _questService.UpdateUserQuestAsync(updateDto, QuestType, cancellationToken);
            return Ok(updatedQuest);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            await _questService.DeleteQuestAsync(id, QuestType, accountId, cancellationToken);
            return NoContent();
        }
    }
}
