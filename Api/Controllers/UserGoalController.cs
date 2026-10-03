using Api.Helpers;
using Application.Quests.Commands.AddQuestCompletion;
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

        /// <summary>
        /// Completes the quest behind a goal. Kept as a convenience for the goal screen; it simply records a
        /// completion, exactly as <c>POST /api/quests/{id}/completions</c> does, and the goal is achieved as
        /// a consequence of the quest's period reaching its target.
        /// </summary>
        [HttpPatch]
        [Route("{id}/completion")]
        public async Task<IActionResult> CompleteUserGoal(int id, CancellationToken cancellationToken = default)
        {
            var userProfileId = JwtHelpers.GetCurrentUserProfileId(User);

            var command = new AddQuestCompletionCommand(QuestId: id, UserProfileId: userProfileId);

            return Ok(await sender.Send(command, cancellationToken));
        }
    }
}
