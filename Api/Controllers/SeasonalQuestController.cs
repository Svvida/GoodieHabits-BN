using Application.Dtos.SeasonalQuest;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/seasonal-quests")]
    public class SeasonalQuestController : ControllerBase
    {
        private readonly ISeasonalQuestService _service;

        public SeasonalQuestController(ISeasonalQuestService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetSeasonalQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _service.GetByIdAsync(id, cancellationToken);

            if (quest is null)
                return NotFound();

            return Ok(quest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonalQuestDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _service.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateSeasonalQuestDto createDto,
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
            [FromBody] PatchSeasonalQuestDto patchDto,
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
            [FromBody] UpdateSeasonalQuestDto updateDto,
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
