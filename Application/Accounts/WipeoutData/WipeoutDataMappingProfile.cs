using AutoMapper;

namespace Application.Accounts.WipeoutData
{
    public class WipeoutDataMappingProfile : Profile
    {
        public WipeoutDataMappingProfile()
        {
            CreateMap<WipeoutDataRequest, WipeoutDataCommand>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());
        }
    }
}
