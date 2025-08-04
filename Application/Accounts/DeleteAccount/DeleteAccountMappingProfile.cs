using AutoMapper;

namespace Application.Accounts.DeleteAccount
{
    public class DeleteAccountMappingProfile : Profile
    {
        public DeleteAccountMappingProfile()
        {
            CreateMap<DeleteAccountRequest, DeleteAccountCommand>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());
        }
    }
}
