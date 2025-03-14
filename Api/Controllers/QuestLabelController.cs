﻿using Application.Dtos.Labels;
using Application.Interfaces;
using Application.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/quest-labels")]
    public class QuestLabelController : ControllerBase
    {
        private readonly IQuestLabelService _questLabelService;

        public QuestLabelController(IQuestLabelService questLabelService)
        {
            _questLabelService = questLabelService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetQuestLabelDto>>> GetUserLabelsAsync(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            return Ok(await _questLabelService.GetUserLabelsAsync(accountId, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            int questLabel = await _questLabelService.CreateLabelAsync(createDto, cancellationToken);

            return Ok(questLabel);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchLabelAsync(int id, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            await _questLabelService.PatchLabelAsync(id, patchDto, accountId, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabelAsync(int id, CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");
            await _questLabelService.DeleteLabelAsync(id, accountId, cancellationToken);
            return Ok();
        }
    }
}
