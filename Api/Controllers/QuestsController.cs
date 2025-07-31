using System.Text.Json;
using Api.Filters;
using Api.Helpers;
using Application.Common;
using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Quests.Commands.CreateQuest;
using Application.Quests.Commands.DeleteQuest;
using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Commands.UpdateQuestCompletion;
using Application.Quests.Queries.GetActiveQuests;
using Application.Quests.Queries.GetQuestById;
using Application.Quests.Queries.GetQuestsByType;
using Application.Quests.Queries.GetQuestsEligibleForGoal;
using Domain.Enum;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/quests")]
    [Authorize]
    public class QuestsController(
        ISender sender,
        IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly ISender _sender = sender;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        [HttpGet("{questType}/{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<BaseGetQuestDto>> GetUserQuestById(
            int id,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var query = new GetQuestByIdQuery(id, questType);
            var questDto = await _sender.Send(query, cancellationToken);

            if (questDto is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Quest not found",
                    Detail = $"Quest with ID {id} was not found"
                });
            }

            return Ok(questDto);
        }

        [HttpGet("{questType}")]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetQuestsByType(
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetQuestsByTypeQuery(accountId, questType);

            var quests = await _sender.Send(query, cancellationToken);
            return Ok(quests);
        }

        [HttpPost("{questType}")]
        public async Task<ActionResult<BaseGetQuestDto>> Create(
            [FromBody] JsonElement dtoAsJson,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            BaseCreateQuestDto? createDto = questType switch
            {
                QuestTypeEnum.OneTime => dtoAsJson.Deserialize<CreateOneTimeQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Daily => dtoAsJson.Deserialize<CreateDailyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Weekly => dtoAsJson.Deserialize<CreateWeeklyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Monthly => dtoAsJson.Deserialize<CreateMonthlyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Seasonal => dtoAsJson.Deserialize<CreateSeasonalQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                _ => throw new InvalidArgumentException($"Quest type '{questType}' is not supported for creation.")
            };

            if (createDto is null)
                return BadRequest("The request body is invalid.");

            var validatorType = typeof(IValidator<>).MakeGenericType(createDto.GetType());

            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator is not null)
            {
                var validationContext = new ValidationContext<object>(createDto);
                var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }
            }

            createDto.AccountId = JwtHelpers.GetCurrentUserId(User);
            createDto.QuestType = questType;

            var command = new CreateQuestCommand(createDto, cancellationToken);

            var createdQuest = await _sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = questType.ToString().ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPatch("{questType}/{id}/completion")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> PatchQuestCompletion(
            int id,
            QuestTypeEnum questType,
            [FromBody] QuestCompletionPatchDto patchDto,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateQuestCompletionCommand(
                id,
                patchDto.IsCompleted,
                questType);
            await _sender.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPut("{questType}/{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<ActionResult<BaseGetQuestDto>> Update(
            int id,
            QuestTypeEnum questType,
            [FromBody] JsonElement dtoAsJson,
            CancellationToken cancellationToken = default)
        {
            BaseUpdateQuestDto? updateDto = questType switch
            {
                QuestTypeEnum.OneTime => dtoAsJson.Deserialize<UpdateOneTimeQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Daily => dtoAsJson.Deserialize<UpdateDailyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Weekly => dtoAsJson.Deserialize<UpdateWeeklyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Monthly => dtoAsJson.Deserialize<UpdateMonthlyQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                QuestTypeEnum.Seasonal => dtoAsJson.Deserialize<UpdateSeasonalQuestDto>(JsonSerializerConfig.CaseInsensitveOptions),
                _ => throw new InvalidArgumentException($"Quest type '{questType}' is not supported for update.")
            };

            if (updateDto is null)
                return BadRequest("The request body is invalid.");

            var validatorType = typeof(IValidator<>).MakeGenericType(updateDto.GetType());

            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator is not null)
            {
                var validationContext = new ValidationContext<object>(updateDto);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }
            }

            updateDto.AccountId = JwtHelpers.GetCurrentUserId(User);
            updateDto.Id = id;
            updateDto.QuestType = questType;

            var command = new UpdateQuestCommand(updateDto, cancellationToken);
            var updatedQuest = await _sender.Send(command, cancellationToken);
            return Ok(updatedQuest);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteQuestCommand(id);

            await _sender.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetActiveQuests(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetActiveQuestsQuery(accountId, cancellationToken);

            var quests = await _sender.Send(query, cancellationToken);

            return Ok(quests);
        }

        [HttpGet("eligible-for-goal")]
        public async Task<ActionResult<IEnumerable<BaseGetQuestDto>>> GetQuestsEligibleForGoal(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);
            var query = new GetQuestsEligibleForGoalQuery(accountId, cancellationToken);
            var quests = await _sender.Send(query, cancellationToken);
            return Ok(quests);
        }
    }
}
