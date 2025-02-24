using Application.Dtos.Accounts;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            // Entity -> DTO
            CreateMap<Account, GetAccountDto>();

            // Create DTO -> Entity
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.HashPassword, opt => opt.MapFrom(src => src.Password));
        }
    }
}
