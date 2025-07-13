using Api.Filters;
using Api.Helpers;
using Application.Commands;
using Application.Dtos.Quests;
using Application.Dtos.UserGoal;
using Application.Interfaces;
using Application.Interfaces.Quests;
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
    public class UserGoalController : ControllerBase
    {
        private readonly IUserGoalService _userGoalService;
        private readonly ILogger<UserGoalController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQuestService _questService;
        private readonly ISender _sender;

        public UserGoalController(
            IUserGoalService userGoalService,
            ILogger<UserGoalController> logger,
            IUnitOfWork unitOfWork,
            IQuestService questService,
            ISender sender)
        {
            _userGoalService = userGoalService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _questService = questService;
            _sender = sender;
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

            var accountId = JwtHelpers.GetCurrentUserId(User);

            var result = await _userGoalService.GetUserActiveGoalByTypeAsync(
                accountId,
                goalTypeEnum,
                cancellationToken);

            if (result == null)
            {
                return NoContent();
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

            var command = new UpdateQuestCompletionCommand(
                id,
                patchDto.IsCompleted,
                quest.QuestType);
            await _sender.Send(command, cancellationToken);
            return Ok();
        }
    }
}
