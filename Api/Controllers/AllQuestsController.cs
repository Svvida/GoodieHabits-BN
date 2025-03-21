﻿using Application.Dtos.Quests;
using Application.Interfaces.Quests;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/all-quests")]
    [Authorize]
    public class AllQuestsController : ControllerBase
    {
        private readonly IQuestService _questMetadataService;
        private readonly ILogger<AllQuestsController> _logger;

        public AllQuestsController(
            IQuestService questMetadataService,
            ILogger<AllQuestsController> logger)
        {
            _questMetadataService = questMetadataService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetAllQuestesAsync(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _questMetadataService.GetActiveQuestsAsync(accountId, cancellationToken);
            return Ok(quests);
        }
    }
}
