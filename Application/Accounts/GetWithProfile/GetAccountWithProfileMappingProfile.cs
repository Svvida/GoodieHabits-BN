using Application.Accounts.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.Accounts.GetWithProfile
{
    public class GetAccountWithProfileMappingProfile : Profile
    {
        public GetAccountWithProfileMappingProfile()
        {
            CreateMap<Account, GetAccountWithProfileResponse>()
                .ForCtorParam(nameof(GetAccountWithProfileResponse.JoinDate), opt => opt.MapFrom(src => src.CreatedAt))
                .ForCtorParam(nameof(GetAccountWithProfileResponse.Preferences), opt => opt.MapFrom(src => new List<AccountPreferencesDto>()));
        }
    }
}
