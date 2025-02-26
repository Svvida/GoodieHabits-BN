using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/one-time-quests")]
    [ApiController]
    public class OneTimeQuestController : ControllerBase
    {
        private readonly IOneTimeQuestService _service;

        public OneTimeQuestController(
            IOneTimeQuestService oneTimeQuestService)
        {
            _service = oneTimeQuestService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetOneTimeQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
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
        public async Task<ActionResult<IEnumerable<GetOneTimeQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _service.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateOneTimeQuestDto createDto,
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
            [FromBody] PatchOneTimeQuestDto patchDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.PatchAsync(id, patchDto, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateOneTimeQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.UpdateAsync(id, updateDto, cancellationToken);
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
