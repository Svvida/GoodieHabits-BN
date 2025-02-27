using Application.Dtos.Quests.MonthlyQuest;
using Application.Interfaces.Quests;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/monthly-quests")]
    public class MonthlyQuestController : ControllerBase
    {
        private readonly IMonthlyQuestService _service;

        public MonthlyQuestController(IMonthlyQuestService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetMonthlyQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            var quest = await _service.GetUserQuestByIdAsync(id, accountId, cancellationToken);
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
        public async Task<ActionResult<IEnumerable<GetMonthlyQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            var quests = await _service.GetAllUserQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateMonthlyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            createDto.AccountId = accountId;

            var createdId = await _service.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(
            int id,
            [FromBody] PatchMonthlyQuestDto patchDto,
            CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            await _service.PatchUserQuestAsync(id, accountId, patchDto, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateMonthlyQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            await _service.UpdateUserQuestAsync(id, accountId, updateDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
