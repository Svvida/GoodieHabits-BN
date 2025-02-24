using Application.Dtos.Quests.QuestMetadata;
using Application.Interfaces.Quests;
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
        public async Task<ActionResult<IEnumerable<GetQuestMetadataDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            return Ok(await _questMetadataService.GetAllQuestsAsync(cancellationToken));
        }
    }
}
