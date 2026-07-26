using Api.Helpers;
using Application.Finance.Categories.Commands.CreateFinanceCategory;
using Application.Finance.Categories.Commands.DeleteFinanceCategories;
using Application.Finance.Categories.Commands.UpdateFinanceCategory;
using Application.Finance.Categories.Dtos;
using Application.Finance.Categories.Queries.GetUserCategoryTree;
using Domain.Enums;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/categories")]
    public class FinanceCategoriesController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FinanceCategoryDto>>> GetCategoryTreeAsync(
            [FromQuery] FinanceTransactionTypeEnum? type = null, CancellationToken cancellationToken = default)
        {
            var query = new GetUserCategoryTreeQuery(User.GetCurrentUserProfileId(), type);
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<ActionResult<FinanceCategoryDto>> CreateAsync(
            [FromBody] CreateFinanceCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<CreateFinanceCategoryCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<FinanceCategoryDto>> UpdateAsync(
            int id, [FromBody] UpdateFinanceCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateFinanceCategoryCommand>(request) with
            {
                CategoryId = id,
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteManyAsync(
            [FromBody] DeleteFinanceCategoriesRequest request, CancellationToken cancellationToken = default)
        {
            var command = new DeleteFinanceCategoriesCommand(request.CategoryIds, User.GetCurrentUserProfileId());
            await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
    }
}
