using Api.Helpers;
using Application.Quests.Commands.UpdateQuestCompletion;
using Application.Quests.Dtos;
using Application.UserGoals.Commands.CreateUserGoal;
using Application.UserGoals.Queries.GetActiveGoalByType;
using Domain.Enums;
using Domain.Interfaces;
using MapsterMapper;
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
            var command = mapper.Map<CreateUserGoalCommand>(request) with { UserProfileId = JwtHelpers.GetCurrentUserProfileId(User) };
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

            var query = new GetActiveGoalByTypeQuery(
                JwtHelpers.GetCurrentUserProfileId(User),
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
            var userProfileId = JwtHelpers.GetCurrentUserProfileId(User);
            var quest = await unitOfWork.Quests.GetUserQuestByIdAsync(id, userProfileId, cancellationToken);

            if (quest is null)
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
                UserProfileId = userProfileId,
                QuestType = quest.QuestType
            };

            await sender.Send(command, cancellationToken);
            return Ok();
        }
    }
}
