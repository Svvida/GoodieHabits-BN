using Application.Common.Interfaces;
using Application.Finance.Settings.Dtos;

namespace Application.Finance.Settings.Queries.GetFinanceSettings
{
    public record GetFinanceSettingsQuery(int UserProfileId) : IQuery<FinanceSettingsDto>;
}
