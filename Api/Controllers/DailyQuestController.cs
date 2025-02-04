using Application.Dtos.DailyQuest;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/daily-quests")]
    public class DailyQuestController : ControllerBase
    {
        private readonly IDailyQuestService _service;

        public DailyQuestController(IDailyQuestService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DailyQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var dailyQuest = await _service.GetByIdAsync(id, cancellationToken);

            if (dailyQuest is null)
                return NotFound();

            return Ok(dailyQuest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _service.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateDailyQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdId = await _service.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(
            int id,
            [FromBody] PatchDailyQuestDto patchDto,
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
            [FromBody] UpdateDailyQuestDto updateDto,
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
