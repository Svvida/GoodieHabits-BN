using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Domain;
using Domain.Exceptions;
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
        private readonly ILogger<QuestsController> _logger;

        public QuestsController(
            IQuestService questService,
            ILogger<QuestsController> logger)
        {
            _questService = questService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _questService.GetActiveQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }
    }
}
