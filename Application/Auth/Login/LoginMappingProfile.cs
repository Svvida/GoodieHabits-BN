using AutoMapper;

namespace Application.Auth.Login
{
    public class LoginMappingProfile : Profile
    {
        public LoginMappingProfile()
        {
            CreateMap<LoginRequest, LoginCommand>();
        }
    }
}
