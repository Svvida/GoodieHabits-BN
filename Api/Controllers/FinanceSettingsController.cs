using Api.Helpers;
using Application.Finance.Settings.Commands.UpdateCurrency;
using Application.Finance.Settings.Dtos;
using Application.Finance.Settings.Queries.GetFinanceSettings;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/finance/settings")]
    public class FinanceSettingsController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<FinanceSettingsDto>> GetAsync(CancellationToken cancellationToken = default)
        {
            var query = new GetFinanceSettingsQuery(User.GetCurrentUserProfileId());
            return Ok(await sender.Send(query, cancellationToken).ConfigureAwait(false));
        }

        [HttpPut("currency")]
        public async Task<ActionResult<FinanceSettingsDto>> UpdateCurrencyAsync(
            [FromBody] UpdateCurrencyRequest request, CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateCurrencyCommand>(request) with
            {
                UserProfileId = User.GetCurrentUserProfileId()
            };
            return Ok(await sender.Send(command, cancellationToken).ConfigureAwait(false));
        }
    }
}
