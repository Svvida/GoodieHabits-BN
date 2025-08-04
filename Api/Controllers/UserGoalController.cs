using Api.Helpers;
using Application.Quests.Dtos;
using Application.Quests.UpdateQuestCompletion;
using Application.UserGoals.CreateUserGoal;
using Application.UserGoals.GetActiveGoalByType;
using AutoMapper;
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
        ISender sender,
        IMapper mapper) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateUserGoal(
            [FromBody] CreateUserGoalRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateUserGoalCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet]
        [Route("active/{goaltype}")]
        public async Task<ActionResult<QuestDetailsDto>> GetActiveGoal(
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

            var result = await sender.Send(query, cancellationToken);

            if (result is null)
                return NoContent();

            return Ok(result);
        }

        [HttpPatch]
        [Route("{id}/completion")]
        public async Task<IActionResult> CompleteUserGoal(int id, [FromBody] UpdateQuestCompletionRequest request, CancellationToken cancellationToken = default)
        {
            var quest = await unitOfWork.Quests.GetByIdAsync(id, cancellationToken);
            var accountId = JwtHelpers.GetCurrentUserId(User);

            if (quest is null || quest.AccountId != accountId)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Quest not found",
                    Detail = $"Quest with ID {id} was not found."
                });
            }

            var command = mapper.Map<UpdateQuestCompletionCommand>(request) with
            {
                QuestId = id,
                AccountId = accountId,
                QuestType = quest.QuestType
            };

            await sender.Send(command, cancellationToken);
            return Ok();
        }
    }
}
