using Mapster;

namespace Application.Auth.Register
{
    public class RegisterMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegisterRequest, RegisterCommand>();
        }
    }
}
