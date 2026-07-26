using Application.Common.Interfaces;
using Application.Finance.Settings.Dtos;

namespace Application.Finance.Settings.Commands.UpdateCurrency
{
    public record UpdateCurrencyCommand(string Currency, int UserProfileId) : ICommand<FinanceSettingsDto>;
}
