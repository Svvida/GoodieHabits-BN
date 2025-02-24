using System.Security.Claims;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/weekly-quests")]
    public class WeeklyQuestController : ControllerBase
    {
        private readonly IWeeklyQuestService _service;

        public WeeklyQuestController(IWeeklyQuestService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetWeeklyQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _service.GetByIdAsync(id, cancellationToken);

            if (quest is null)
                return NotFound();

            return Ok(quest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetWeeklyQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _service.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateWeeklyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out var accountId))
                return Unauthorized("Account id is missing or invalid");

            createDto.AccountId = accountId;
            var createdId = await _service.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(
            int id,
            [FromBody] PatchWeeklyQuestDto patchDto,
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
            [FromBody] UpdateWeeklyQuestDto updateDto,
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
