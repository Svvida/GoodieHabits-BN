using Api.Helpers;
using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/all-quests")]
    [Authorize]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _questService;

        public QuestsController(
            IQuestService questService)
        {
            _questService = questService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var quests = await _questService.GetActiveQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }

        [HttpGet("eligible-for-goals")]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetQuestsEligibleForGoalAsync(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);
            var quests = await _questService.GetQuestEligibleForGoalAsync(accountId, cancellationToken);
            return Ok(quests);
        }
    }
}
