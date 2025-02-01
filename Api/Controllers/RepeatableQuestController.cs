using Application.Dtos.RepeatableQuest;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/repeatable-quests")]
    public class RepeatableQuestController : ControllerBase
    {
        private readonly IRepeatableQuestService _service;

        public RepeatableQuestController(IRepeatableQuestService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RepeatableQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _service.GetByIdAsync(id, cancellationToken);
            if (quest is null)
                return NotFound($"Quest with ID: {id} not found.");

            return Ok(quest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepeatableQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _service.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateRepeatableQuestDto createDto,
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
            [FromBody] PatchRepeatableQuestDto patchDto,
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
            [FromBody] UpdateRepeatableQuestDto updateDto,
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

        [HttpGet("by-types")]
        public async Task<ActionResult<IEnumerable<RepeatableQuestDto>>> GetByTypes(
            [FromQuery] List<string> types,
            CancellationToken cancellationToken = default)
        {
            if (types is null || types.Count == 0)
                return BadRequest("At least one type must be provided");

            var quests = await _service.GetByTypesAsync(types, cancellationToken);
            return Ok(quests);
        }
    }
}
