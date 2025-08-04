using AutoMapper;

namespace Application.Accounts.ChangePassword
{
    public class ChangePasswordMappingProfile : Profile
    {
        public ChangePasswordMappingProfile()
        {
            CreateMap<ChangePasswordRequest, ChangePasswordCommand>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());
        }
    }
}
