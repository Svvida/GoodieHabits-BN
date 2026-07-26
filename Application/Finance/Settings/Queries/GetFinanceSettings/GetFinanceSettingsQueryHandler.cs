using Application.Finance.Settings.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Settings.Queries.GetFinanceSettings
{
    public class GetFinanceSettingsQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetFinanceSettingsQuery, FinanceSettingsDto>
    {
        public async Task<FinanceSettingsDto> Handle(GetFinanceSettingsQuery request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("User profile not found.");

            return new FinanceSettingsDto { Currency = profile.Currency };
        }
    }
}
