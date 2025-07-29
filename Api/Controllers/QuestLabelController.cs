using Api.Filters;
using Api.Helpers;
using Application.Dtos.Labels;
using Application.Interfaces;
using Application.QuestLabels.Queries.GetUserLabels;
using MediatR;
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
        private readonly ISender _sender;

        public QuestLabelController(IQuestLabelService questLabelService, ISender sender)
        {
            _questLabelService = questLabelService;
            _sender = sender;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetQuestLabelDto>>> GetUserLabelsAsync(CancellationToken cancellationToken = default)
        {
            var accountId = JwtHelpers.GetCurrentUserId(User);

            var query = new GetUserLabelsQuery(accountId);

            return Ok(await _sender.Send(query, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateLabelAsync(CreateQuestLabelDto createDto, CancellationToken cancellationToken = default)
        {
            createDto.AccountId = JwtHelpers.GetCurrentUserId(User);

            int questLabel = await _questLabelService.CreateLabelAsync(createDto, cancellationToken);

            return Ok(questLabel);
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(QuestLabelAuthorizationFilter))]
        public async Task<IActionResult> PatchLabelAsync(int id, PatchQuestLabelDto patchDto, CancellationToken cancellationToken = default)
        {
            await _questLabelService.PatchLabelAsync(id, patchDto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(QuestLabelAuthorizationFilter))]
        public async Task<IActionResult> DeleteLabelAsync(int id, CancellationToken cancellationToken = default)
        {
            await _questLabelService.DeleteLabelAsync(id, cancellationToken);
            return Ok();
        }
    }
}
