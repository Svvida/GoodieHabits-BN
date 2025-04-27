using Application.Dtos.Quests;
using Application.Dtos.UserGoal;
using Application.Interfaces;
using Domain;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLambdaApi.Filters;

namespace MyLambdaApi.Controllers
{
    [ApiController]
    [Route("api/goals")]
    [Authorize]
    public class UserGoalController : ControllerBase
    {
        private readonly IUserGoalService _userGoalService;
        private readonly ILogger<UserGoalController> _logger;

        public UserGoalController(IUserGoalService userGoalService, ILogger<UserGoalController> logger)
        {
            _userGoalService = userGoalService;
            _logger = logger;
        }

        [HttpPost]
        [Route("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> CreateUserGoal(
            int id,
            [FromBody] CreateUserGoalDto createUserGoalDto,
            CancellationToken cancellationToken = default)
        {
            createUserGoalDto.QuestId = id;
            await _userGoalService.CreateUserGoalAsync(createUserGoalDto, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [Route("active/{goalType}")]
        public async Task<ActionResult<BaseGetQuestDto>> GetActiveGoal(
            string goalType,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<GoalTypeEnum>(goalType, true, out var goalTypeEnum))
            {
                return BadRequest($"Invalid goal type: {goalType}. Valid values are Daily, Weekly, Monthly, Yearly.");
            }

            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var result = await _userGoalService.GetUserActiveGoalByTypeAsync(
                accountId,
                goalTypeEnum,
                cancellationToken);

            if (result == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Goal not found",
                    Detail = $"Goal of type {goalType} was not found for the user."
                });
            }
            return Ok(result);
        }

        [HttpPatch]
        [Route("active/{goalType}")]
        public async Task<IActionResult> AbandonActiveGoal(
            string goalType,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<GoalTypeEnum>(goalType, true, out var goalTypeEnum))
            {
                return BadRequest($"Invalid goal type: {goalType}. Valid values are Daily, Weekly, Monthly, Yearly.");
            }

            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            await _userGoalService.AbandonUserGoalAsync(accountId, goalTypeEnum, cancellationToken);

            return NoContent();
        }
    }
}
