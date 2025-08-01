using Api.Filters;
using Api.Helpers;
using Application.Dtos.Quests;
using Application.Quests.Commands.UpdateQuestCompletion;
using Application.UserGoals.Commands.CreateUserGoal;
using Application.UserGoals.Queries.GetActiveGoalByType;
using Domain.Enum;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/goals")]
    [Authorize]
    public class UserGoalController(
        IUnitOfWork unitOfWork,
        ISender sender) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ISender _sender = sender;

        [HttpPost]
        [Route("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> CreateUserGoal(
            int id,
            [FromBody] CreateUserGoalCommand command,
            CancellationToken cancellationToken = default)
        {
            command.QuestId = id;
            command.AccountId = JwtHelpers.GetCurrentUserId(User);
            await _sender.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("active/{goaltype}")]
        public async Task<ActionResult<BaseGetQuestDto>> GetActiveGoal(
            [FromRoute] string goaltype,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<GoalTypeEnum>(goaltype, true, out var goalTypeEnum))
            {
                return BadRequest($"Invalid goal type: {goaltype}. Valid values are Daily, Weekly, Monthly, Yearly.");
            }

            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetActiveGoalByTypeQuery(
                accountId,
                goalTypeEnum);

            var result = await _sender.Send(query, cancellationToken);

            if (result is null)
                return NoContent();

            return Ok(result);
        }

        [HttpPatch]
        [Route("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> CompleteUserGoal(int id, QuestCompletionPatchDto patchDto, CancellationToken cancellationToken = default)
        {
            var quest = await _unitOfWork.Quests.GetByIdAsync(id, cancellationToken);

            if (quest is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Quest not found",
                    Detail = $"Quest with ID {id} was not found."
                });
            }

            var command = new UpdateQuestCompletionCommand(
                id,
                patchDto.IsCompleted,
                quest.QuestType);
            await _sender.Send(command, cancellationToken);
            return Ok();
        }
    }
}
