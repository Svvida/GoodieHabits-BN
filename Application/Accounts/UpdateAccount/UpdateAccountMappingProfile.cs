using AutoMapper;

namespace Application.Accounts.UpdateAccount
{
    public class UpdateAccountMappingProfile : Profile
    {
        public UpdateAccountMappingProfile()
        {
            CreateMap<UpdateAccountRequest, UpdateAccountCommand>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());
        }
    }
}
