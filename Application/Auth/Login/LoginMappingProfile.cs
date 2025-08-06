using Mapster;

namespace Application.Auth.Login
{
    public class LoginMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<LoginRequest, LoginCommand>();
        }
    }
}
