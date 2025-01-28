using Application.Dtos.OneTimeQuest;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OneTimeQuestController : ControllerBase
    {
        private readonly IOneTimeQuestService _oneTimeQuestService;
        private readonly ILogger<OneTimeQuestController> _logger;

        public OneTimeQuestController(
            IOneTimeQuestService oneTimeQuestService,
            ILogger<OneTimeQuestController> logger)
        {
            _oneTimeQuestService = oneTimeQuestService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OneTimeQuestDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = await _oneTimeQuestService.GetByIdAsync(id, cancellationToken);

            if (oneTimeQuest == null)
            {
                return NotFound();
            }

            return Ok(oneTimeQuest);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var quests = await _oneTimeQuestService.GetAllAsync(cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdId = await _oneTimeQuestService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody] PatchOneTimeQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            try
            {
                await _oneTimeQuestService.PatchAsync(id, patchDto, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _oneTimeQuestService.UpdateAsync(id, updateDto, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating OneTimeQuest with Id: {Id}", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _oneTimeQuestService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
