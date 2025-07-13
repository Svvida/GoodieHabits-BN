using Api.Filters;
using Api.Helpers;
using Application.Commands;
using Application.Dtos.Quests;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/weekly-quests")]
    [Authorize]
    public class WeeklyQuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private readonly ISender _sender;
        private static QuestTypeEnum QuestType => QuestTypeEnum.Weekly;

        public WeeklyQuestController(IQuestService questService, ISender sender)
        {
            _questService = questService;
            _sender = sender;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetWeeklyQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
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
        public async Task<ActionResult<IEnumerable<GetWeeklyQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var quests = await _questService.GetAllUserQuestsByTypeAsync(accountId, QuestType, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<GetWeeklyQuestDto>> Create(
            [FromBody] CreateWeeklyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            createDto.AccountId = JwtHelpers.GetCurrentUserId(User);

            var createdQuest = await _questService.CreateUserQuestAsync(createDto, QuestType, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdQuest.Id }, createdQuest);
        }

        [HttpPatch("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetWeeklyQuestDto>> PatchQuestCompletion(
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
        public async Task<ActionResult<GetWeeklyQuestDto>> Update(
            int id,
            [FromBody] UpdateWeeklyQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            updateDto.AccountId = JwtHelpers.GetCurrentUserId(User);
            updateDto.Id = id;
            var updatedQuest = await _questService.UpdateUserQuestAsync(updateDto, QuestType, cancellationToken);
            return Ok(updatedQuest);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            await _questService.DeleteQuestAsync(id, QuestType, accountId, cancellationToken);
            return NoContent();
        }
    }
}
