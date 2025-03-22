using Api.Filters;
using Application.Dtos.Quests.DailyQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using Domain;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/daily-quests")]
    [Authorize]
    public class DailyQuestController : ControllerBase
    {
        private readonly IDailyQuestService _service;
        private readonly IQuestLabelService _questLabelService;
        private readonly ILogger<DailyQuestController> _logger;

        public DailyQuestController(
            IDailyQuestService service,
            IQuestLabelService questLabelService,
            ILogger<DailyQuestController> logger)
        {
            _service = service;
            _questLabelService = questLabelService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetDailyQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
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
        public async Task<ActionResult<IEnumerable<GetDailyQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _service.GetAllUserQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateDailyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                return Unauthorized("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            //var userLabelDtos = await _questLabelService.GetUserLabelsAsync(accountId, cancellationToken);
            //HashSet<int> userLabelsIds = userLabelDtos.Select(label => label.Id).ToHashSet();

            //if (!createDto.Labels.IsSubsetOf(userLabelsIds))
            //    return BadRequest("One or more og the specified labels do not belong to the user.");


            var createdId = await _service.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> UpdatePartial(
            int id,
            [FromBody] PatchDailyQuestDto patchDto,
            CancellationToken cancellationToken = default)
        {
            await _service.PatchUserQuestAsync(id, patchDto, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateDailyQuestDto updateDto,
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
