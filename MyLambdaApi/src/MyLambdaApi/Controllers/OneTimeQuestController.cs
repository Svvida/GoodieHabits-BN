﻿using Application.Dtos.Quests;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using Domain;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLambdaApi.Filters;

namespace MyLambdaApi.Controllers
{
    [Route("api/one-time-quests")]
    [ApiController]
    [Authorize]
    public class OneTimeQuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private static QuestTypeEnum QuestType => QuestTypeEnum.OneTime;

        public OneTimeQuestController(IQuestService questService)
        {
            _questService = questService;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<GetOneTimeQuestDto>> GetUserQuestById(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _questService.GetUserQuestByIdAsync(id, QuestType, cancellationToken);

            if (quest is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Quest not found",
                    Detail = $"Quest with ID {id} was not found"
                });
            }

            return Ok(quest);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOneTimeQuestDto>>> GetAllUserQuests(CancellationToken cancellationToken = default)
        {
            string? accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            var quests = await _questService.GetAllUserQuestsByTypeAsync(accountId, QuestType, cancellationToken);
            return Ok(quests);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateOneTimeQuestDto createDto,
            CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                throw new UnauthorizedException("Invalid access token: missing account identifier.");

            createDto.AccountId = accountId;

            var createdId = await _questService.CreateUserQuestAsync(createDto, QuestType, cancellationToken);
            return CreatedAtAction(nameof(GetUserQuestById), new { id = createdId }, new { id = createdId });
        }

        [HttpPatch("{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> PatchQuestCompletion(
            int id,
            [FromBody] QuestCompletionPatchDto patchDto,
            CancellationToken cancellationToken = default)
        {
            patchDto.Id = id;
            await _questService.UpdateQuestCompletionAsync(patchDto, QuestType, cancellationToken);
            return NoContent();

        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> UpdateUserQuest(
            int id,
            [FromBody] UpdateOneTimeQuestDto updateDto,
            CancellationToken cancellationToken = default)
        {
            updateDto.Id = id;
            await _questService.UpdateUserQuestAsync(updateDto, QuestType, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var accountIdString = User.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountIdString) || !int.TryParse(accountIdString, out int accountId))
                return Unauthorized("Invalid access token: missing account identifier.");

            await _questService.DeleteQuestAsync(id, QuestType, accountId, cancellationToken);
            return NoContent();
        }
    }
}
