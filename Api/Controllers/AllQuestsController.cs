using Application.Dtos.QuestMetadata;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/all-quests")]
    public class AllQuestsController : ControllerBase
    {
        private readonly IQuestMetadataService _questMetadataService;

        public AllQuestsController(IQuestMetadataService questMetadataService)
        {
            _questMetadataService = questMetadataService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestMetadataDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            return Ok(await _questMetadataService.GetAllQuestsAsync(cancellationToken));
        }
    }
}
