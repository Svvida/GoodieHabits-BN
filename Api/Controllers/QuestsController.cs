using Api.Filters;
using Api.Helpers;
using Application.Quests.CreateQuest;
using Application.Quests.DeleteQuest;
using Application.Quests.Dtos;
using Application.Quests.GetActiveQuests;
using Application.Quests.GetQuestById;
using Application.Quests.GetQuestsByType;
using Application.Quests.GetQuestsEligibleForGoal;
using Application.Quests.UpdateQuest;
using Application.Quests.UpdateQuestCompletion;
using Domain.Enum;
using MapsterMapper;
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
        IMapper mapper) : ControllerBase
    {
        #region Shared Methods
        [HttpGet("{questType}/{id}")]
        public async Task<ActionResult<QuestDetailsDto>> GetUserQuestById(
            int id,
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var query = new GetQuestByIdQuery(id, questType, JwtHelpers.GetCurrentUserId(User));
            var questDto = await sender.Send(query, cancellationToken);

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
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetQuestsByType(
            QuestTypeEnum questType,
            CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetQuestsByTypeQuery(accountId, questType);

            var quests = await sender.Send(query, cancellationToken);
            return Ok(quests);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestAuthorizationFilter))]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteQuestCommand(id);

            await sender.Send(command, cancellationToken);

            return NoContent();
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetActiveQuests(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetActiveQuestsQuery(accountId, cancellationToken);

            var quests = await sender.Send(query, cancellationToken);

            return Ok(quests);
        }

        [HttpGet("eligible-for-goal")]
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetQuestsEligibleForGoal(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);
            var query = new GetQuestsEligibleForGoalQuery(accountId, cancellationToken);
            var quests = await sender.Send(query, cancellationToken);
            return Ok(quests);
        }

        [HttpPatch("{questType}/{id}/completion")]
        public async Task<IActionResult> PatchQuestCompletion(
            int id,
            QuestTypeEnum questType,
            [FromBody] UpdateQuestCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateQuestCompletionCommand>(request) with
            {
                QuestId = id,
                QuestType = questType,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        #endregion

        #region OneTime Quests
        [HttpPost("one-time")]
        public async Task<ActionResult<OneTimeQuestDetailsDto>> CreateOneTimeQuest(
            [FromBody] CreateOneTimeQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateOneTimeQuestCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = createdQuest.QuestType.ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPut("one-time/{id}")]
        public async Task<ActionResult<OneTimeQuestDetailsDto>> UpdateOneTimeQuest(
            int id,
            [FromBody] UpdateOneTimeQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var updateDto = mapper.Map<UpdateOneTimeQuestCommand>(request) with
            {
                QuestId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var updatedQuest = await sender.Send(updateDto, cancellationToken);
            return Ok(updatedQuest);
        }
        #endregion

        #region Daily Quests
        [HttpPost("daily")]
        public async Task<ActionResult<DailyQuestDetailsDto>> CreateDailyQuest(
            [FromBody] CreateDailyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateDailyQuestCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = createdQuest.QuestType.ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPut("daily/{id}")]
        public async Task<ActionResult<DailyQuestDetailsDto>> UpdateDailyQuest(
            int id,
            [FromBody] UpdateDailyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var updateDto = mapper.Map<UpdateDailyQuestCommand>(request) with
            {
                QuestId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var updatedQuest = await sender.Send(updateDto, cancellationToken);
            return Ok(updatedQuest);
        }
        #endregion

        #region Weekly Quests
        [HttpPost("weekly")]
        public async Task<ActionResult<WeeklyQuestDetailsDto>> CreateWeeklyQuest(
            [FromBody] CreateWeeklyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateWeeklyQuestCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = createdQuest.QuestType.ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPut("weekly/{id}")]
        public async Task<ActionResult<WeeklyQuestDetailsDto>> UpdateWeeklyQuest(
            int id,
            [FromBody] UpdateWeeklyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var updateDto = mapper.Map<UpdateWeeklyQuestCommand>(request) with
            {
                QuestId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var updatedQuest = await sender.Send(updateDto, cancellationToken);
            return Ok(updatedQuest);
        }
        #endregion

        #region Monthly Quests
        [HttpPost("monthly")]
        public async Task<ActionResult<MonthlyQuestDetailsDto>> CreateMonthlyQuest(
            [FromBody] CreateMonthlyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateMonthlyQuestCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = createdQuest.QuestType.ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPut("monthly/{id}")]
        public async Task<ActionResult<MonthlyQuestDetailsDto>> UpdateMonthlyQuest(
            int id,
            [FromBody] UpdateMonthlyQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var updateDto = mapper.Map<UpdateMonthlyQuestCommand>(request) with
            {
                QuestId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var updatedQuest = await sender.Send(updateDto, cancellationToken);
            return Ok(updatedQuest);
        }
        #endregion

        #region Seasonal Quests
        [HttpPost("seasonal")]
        public async Task<ActionResult<SeasonalQuestDetailsDto>> CreateSeasonalQuest(
            [FromBody] CreateSeasonalQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateSeasonalQuestCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            var routeValues = new
            {
                questType = createdQuest.QuestType.ToLowerInvariant(),
                id = createdQuest.Id
            };

            return CreatedAtAction(nameof(GetUserQuestById), routeValues, createdQuest);
        }

        [HttpPut("seasonal/{id}")]
        public async Task<ActionResult<SeasonalQuestDetailsDto>> UpdateSeasonalQuest(
            int id,
            [FromBody] UpdateSeasonalQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var updateDto = mapper.Map<UpdateSeasonalQuestCommand>(request) with
            {
                QuestId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var updatedQuest = await sender.Send(updateDto, cancellationToken);
            return Ok(updatedQuest);
        }
        #endregion
    }
}
