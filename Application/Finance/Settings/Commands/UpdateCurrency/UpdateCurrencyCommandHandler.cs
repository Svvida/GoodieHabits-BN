using Application.Finance.Settings.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Settings.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCurrencyCommand, FinanceSettingsDto>
    {
        public async Task<FinanceSettingsDto> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("User profile not found.");

            profile.UpdateCurrency(request.Currency);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new FinanceSettingsDto { Currency = profile.Currency };
        }
    }
}
