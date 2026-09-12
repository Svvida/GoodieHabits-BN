using Api.Helpers;
using Application.Quests.Commands.AddQuestCompletion;
using Application.Quests.Commands.CreateQuest;
using Application.Quests.Commands.DeleteQuest;
using Application.Quests.Commands.RemoveQuestCompletion;
using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Application.Quests.Queries.GetActiveQuests;
using Application.Quests.Queries.GetCatchUp;
using Application.Quests.Queries.GetHabitsOverview;
using Application.Quests.Queries.GetQuestAnalytics;
using Application.Quests.Queries.GetQuestById;
using Application.Quests.Queries.GetQuests;
using Application.Quests.Queries.GetQuestsEligibleForGoal;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// One set of routes for every quest. The five per-type create/update pairs are gone — the recurrence is
    /// data in the body now, not a path segment.
    /// </summary>
    [ApiController]
    [Route("api/quests")]
    [Authorize]
    public class QuestsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<QuestDetailsDto>> CreateQuest(
            [FromBody] CreateQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateQuestCommand>(request) with
            {
                UserProfileId = JwtHelpers.GetCurrentUserProfileId(User)
            };

            var createdQuest = await sender.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetQuestById), new { id = createdQuest.Id }, createdQuest);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<QuestDetailsDto>> UpdateQuest(
            int id,
            [FromBody] UpdateQuestRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateQuestCommand>(request) with
            {
                QuestId = id,
                UserProfileId = JwtHelpers.GetCurrentUserProfileId(User)
            };

            return Ok(await sender.Send(command, cancellationToken));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuestDetailsDto>> GetQuestById(int id, CancellationToken cancellationToken = default)
        {
            var questDto = await sender.Send(new GetQuestByIdQuery(id, JwtHelpers.GetCurrentUserProfileId(User)), cancellationToken);

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

        /// <summary>
        /// All of the caller's quests. Filter by <c>unit</c> ("Day", "Week", "Month", "Year", "None"), or by
        /// <c>legacyType</c> ("Daily", "Weekly", "Monthly", "OneTime", "Seasonal") to reproduce the old
        /// per-type screens exactly while the client catches up.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetQuests(
            [FromQuery] string? unit = null,
            [FromQuery] string? legacyType = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetQuestsQuery(JwtHelpers.GetCurrentUserProfileId(User), unit, legacyType);

            return Ok(await sender.Send(query, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await sender.Send(new DeleteQuestCommand(id, JwtHelpers.GetCurrentUserProfileId(User)), cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Everything due today, each row carrying its current period's progress and target. Also the call
        /// that brings the caller's derived state up to date for the day.
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetActiveQuests(CancellationToken cancellationToken = default)
        {
            var query = new GetActiveQuestsQuery(JwtHelpers.GetCurrentUserProfileId(User));

            return Ok(await sender.Send(query, cancellationToken));
        }

        /// <summary>
        /// Records one act of doing the quest. Send it repeatedly for a target above one. Pass
        /// <c>completedOn</c> to backfill a forgotten tap, and a stable <c>clientRequestId</c> so a retry
        /// cannot record twice.
        /// </summary>
        [HttpPost("{id:int}/completions")]
        public async Task<ActionResult<QuestCompletionResponse>> AddCompletion(
            int id,
            [FromBody] AddQuestCompletionRequest? request,
            CancellationToken cancellationToken = default)
        {
            var command = new AddQuestCompletionCommand(
                QuestId: id,
                UserProfileId: JwtHelpers.GetCurrentUserProfileId(User),
                Amount: request?.Amount,
                CompletedOn: request?.CompletedOn,
                ClientRequestId: request?.ClientRequestId,
                Note: request?.Note);

            return Ok(await sender.Send(command, cancellationToken));
        }

        [HttpDelete("{id:int}/completions/{completionId:int}")]
        public async Task<ActionResult<QuestDetailsDto>> RemoveCompletion(
            int id,
            int completionId,
            CancellationToken cancellationToken = default)
        {
            var command = new RemoveQuestCompletionCommand(id, completionId, JwtHelpers.GetCurrentUserProfileId(User));

            return Ok(await sender.Send(command, cancellationToken));
        }

        /// <summary>
        /// Periods from the last couple of days that a tap would still fix — the "did you do these?" card.
        /// An empty list means there is nothing to ask about.
        /// </summary>
        /// <param name="includeCompleted">
        /// Also return periods in the window that are already done, so a catch-up tap made in an earlier
        /// session can still be undone. Off by default, which keeps "empty means hide the card" true.
        /// </param>
        [HttpGet("catch-up")]
        public async Task<ActionResult<GetCatchUpResponse>> GetCatchUp(
            [FromQuery] bool includeCompleted = false,
            CancellationToken cancellationToken = default)
        {
            var query = new GetCatchUpQuery(JwtHelpers.GetCurrentUserProfileId(User), includeCompleted);

            return Ok(await sender.Send(query, cancellationToken));
        }

        /// <summary>
        /// Completion analytics for one repeating quest: windowed summary, calendar cells, a trend series,
        /// and per-weekday and per-hour breakdowns. Dates are inclusive calendar dates and default to the
        /// last 90 days in the user's own timezone.
        /// </summary>
        [HttpGet("{questId:int}/analytics")]
        public async Task<ActionResult<GetQuestAnalyticsResponse>> GetQuestAnalytics(
            int questId,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] AnalyticsGranularityEnum granularity = AnalyticsGranularityEnum.Week,
            CancellationToken cancellationToken = default)
        {
            var query = new GetQuestAnalyticsQuery(
                questId,
                JwtHelpers.GetCurrentUserProfileId(User),
                from,
                to,
                granularity);

            return Ok(await sender.Send(query, cancellationToken));
        }

        /// <summary>
        /// Cross-quest analytics: a per-habit summary, a combined roll-up and a per-day completion-rate
        /// series. Defaults to the last 30 days in the user's own timezone.
        /// </summary>
        [HttpGet("analytics/overview")]
        public async Task<ActionResult<GetHabitsOverviewResponse>> GetHabitsOverview(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetHabitsOverviewQuery(JwtHelpers.GetCurrentUserProfileId(User), from, to);

            return Ok(await sender.Send(query, cancellationToken));
        }

        [HttpGet("eligible-for-goal")]
        public async Task<ActionResult<IEnumerable<QuestDetailsDto>>> GetQuestsEligibleForGoal(CancellationToken cancellationToken = default)
        {
            var query = new GetQuestsEligibleForGoalQuery(JwtHelpers.GetCurrentUserProfileId(User), cancellationToken);

            return Ok(await sender.Send(query, cancellationToken));
        }
    }
}
