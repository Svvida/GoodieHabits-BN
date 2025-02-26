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
        private readonly IOneTimeQuestService _oneTimeQuestService;

        public OneTimeQuestController(
            IOneTimeQuestService oneTimeQuestService)
        {
            _oneTimeQuestService = oneTimeQuestService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetOneTimeQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = await _oneTimeQuestService.GetByIdAsync(id, cancellationToken);

            if (oneTimeQuest is null)
                return NotFound();

            return Ok(oneTimeQuest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOneTimeQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _oneTimeQuestService.GetAllAsync(cancellationToken);
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

            var createdId = await _oneTimeQuestService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(
            int id,
            [FromBody] PatchOneTimeQuestDto patchDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _oneTimeQuestService.PatchAsync(id, patchDto, cancellationToken);
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

            await _oneTimeQuestService.UpdateAsync(id, updateDto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _oneTimeQuestService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
