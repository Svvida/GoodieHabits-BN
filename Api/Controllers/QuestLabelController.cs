using Api.Filters;
using Api.Helpers;
using Application.QuestLabels.Commands.CreateQuestLabel;
using Application.QuestLabels.Commands.DeleteQuestLabel;
using Application.QuestLabels.Commands.PatchQuestLabel;
using Application.QuestLabels.Queries.GetUserLabels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/quest-labels")]
    public class QuestLabelController(ISender sender) : ControllerBase
    {
        private readonly ISender _sender = sender;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetQuestLabelDto>>> GetUserLabelsAsync(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetUserLabelsQuery(accountId);

            return Ok(await _sender.Send(query, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<GetQuestLabelDto>> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default)
        {
            createDto.AccountId = JwtHelpers.GetCurrentUserId(User);

            var command = new CreateQuestLabelCommand(createDto);

            var questLabel = await _sender.Send(command, cancellationToken);

            return Ok(questLabel);
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(QuestLabelAuthorizationFilter))]
        public async Task<ActionResult<GetQuestLabelDto>> PatchLabelAsync(int id, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default)
        {
            patchDto.Id = id;
            patchDto.AccountId = JwtHelpers.GetCurrentUserId(User);

            var command = new PatchQuestLabelCommand(patchDto);
            var label = await _sender.Send(command, cancellationToken);
            return Ok(label);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestLabelAuthorizationFilter))]
        public async Task<IActionResult> DeleteLabelAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteQuestLabelCommand(id);
            await _sender.Send(command, cancellationToken);
            return Ok();
        }
    }
}
