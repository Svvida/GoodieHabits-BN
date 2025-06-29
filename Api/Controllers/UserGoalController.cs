using Api.Filters;
using Application.Dtos.Quests;
using Application.Dtos.UserGoal;
using Application.Interfaces;
using Application.Interfaces.Quests;
using Domain;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/goals")]
    [Authorize]
    public class UserGoalController : ControllerBase
    {
        private readonly IUserGoalService _userGoalService;
        private readonly ILogger<UserGoalController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuestService _questService;

        public UserGoalController(
            IUserGoalService userGoalService,
            ILogger<UserGoalController> logger,
            IUnitOfWork unitOfWork,
            IQuestService questService)
        {
            _userGoalService = userGoalService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _questService = questService;
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
        [Route("active/{goaltype}")]
        public async Task<ActionResult<BaseGetQuestDto>> GetActiveGoal(
            [FromRoute] string goaltype,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<GoalTypeEnum>(goaltype, true, out var goalTypeEnum))
            {
                return BadRequest($"Invalid goal type: {goaltype}. Valid values are Daily, Weekly, Monthly, Yearly.");
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
                    Detail = $"Goal of type {goaltype} was not found for the user."
                });
            }
            return Ok(result);
        }

        [HttpPatch]
        [Route("{id}/complete")]
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

            patchDto.Id = id;

            await _questService.UpdateQuestCompletionAsync(patchDto, quest.QuestType, cancellationToken);

            return Ok();
        }
    }
}
