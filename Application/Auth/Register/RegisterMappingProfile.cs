using AutoMapper;

namespace Application.Auth.Register
{
    public class RegisterMappingProfile : Profile
    {
        public RegisterMappingProfile()
        {
            CreateMap<RegisterRequest, RegisterCommand>()
                .ForMember(dest => dest.TimeZoneId, opt => opt.Ignore());
        }
    }
}
