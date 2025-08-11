using Application.Accounts.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Accounts.Queries.GetWithProfile
{
    public class GetAccountWithProfileMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Account, GetAccountWithProfileResponse>()
                .Map(dest => dest.JoinDate, src => src.CreatedAt)
                .Map(dest => dest.Preferences, src => new List<AccountPreferencesDto>());
        }
    }
}
