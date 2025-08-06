using Api.Helpers;
using Application.QuestLabels.CreateQuestLabel;
using Application.QuestLabels.DeleteQuestLabel;
using Application.QuestLabels.Dtos;
using Application.QuestLabels.GetUserLabels;
using Application.QuestLabels.UpdateQuestLabel;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/quest-labels")]
    public class QuestLabelController(ISender sender, IMapper mapper) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestLabelDto>>> GetUserLabelsAsync(CancellationToken cancellationToken = default)
        {
            var query = new GetUserLabelsQuery(JwtHelpers.GetCurrentUserId(User));
            return Ok(await sender.Send(query, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<CreateQuestLabelResponse>> CreateLabelAsync([FromBody] CreateQuestLabelRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateQuestLabelCommand>(request) with
            {
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };

            var questLabel = await sender.Send(command, cancellationToken);

            return Ok(questLabel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateQuestLabelResponse>> UpdateQuestLabelAsync(int id, [FromBody] UpdateQuestLabelRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateQuestLabelCommand>(request) with
            {
                LabelId = id,
                AccountId = JwtHelpers.GetCurrentUserId(User)
            };
            var label = await sender.Send(command, cancellationToken);
            return Ok(label);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestLabelAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteQuestLabelCommand(id, JwtHelpers.GetCurrentUserId(User));
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
